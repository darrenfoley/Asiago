using Asiago.Controllers.Attributes;
using Asiago.Core.Twitch;
using Asiago.Core.Twitch.EventSub;
using Asiago.Core.Twitch.EventSub.Models;
using Asiago.Invocables.Twitch;
using Coravel.Cache.Interfaces;
using Coravel.Invocable;
using Coravel.Queuing.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Asiago.Controllers
{
    /// <summary>
    /// API Controller for handling webhook requests from Twitch.
    /// </summary>
    [Route("api/webhooks/[controller]")]
    [ApiController]
    [EnableRequestBodyBuffering]
    public class TwitchController : ControllerBase
    {
        private const string _messageIdCacheKeyPrefix = "twitchMessageId-";
        private static readonly TimeSpan _messageIdCacheExpirationHours = TimeSpan.FromHours(3);
        private static readonly TimeSpan _messageExpirationMinutes = TimeSpan.FromMinutes(10);

        private readonly ILogger<TwitchController> _logger;
        private readonly TwitchOptions _twitchOptions;
        private readonly IQueue _queue;
        private readonly ICache _cache;

        public TwitchController(ILoggerFactory loggerFactory, IOptions<TwitchOptions> twitchOptions, IQueue queue, ICache cache)
        {
            _logger = loggerFactory.CreateLogger<TwitchController>();
            _twitchOptions = twitchOptions.Value;
            _queue = queue;
            _cache = cache;
        }

        /// <summary>
        /// Handle verification challenge requests from Twitch in order to prove we own the event handler endpoint.
        /// </summary>
        [HttpPost]
        [RequireHeader(WebhookRequestHeaders.MessageType, WebhookRequestMessageTypes.WebhookCallbackVerification)]
        public async Task<IActionResult> PostCallbackVerification([FromBody] VerificationNotificationPayload payload)
        {
            VerifyMessageResult verifyResult = await VerifyMessageAsync();
            if (!verifyResult.Verified)
            {
                return verifyResult.ErrorActionResult ?? BadRequest();
            }

            return Ok(payload.Challenge);
        }

        /// <summary>
        /// Handle subscription revocation requests from Twitch.
        /// </summary>
        [HttpPost]
        [RequireHeader(WebhookRequestHeaders.MessageType, WebhookRequestMessageTypes.Revocation)]
        public async Task<IActionResult> PostRevocation([FromBody] RevocationNotificationPayload payload)
        {
            VerifyMessageResult verifyResult = await VerifyMessageAsync();
            if (!verifyResult.Verified)
            {
                return verifyResult.ErrorActionResult ?? BadRequest();
            }

            if (!verifyResult.Duplicate)
            {
                _queue.QueueInvocableWithPayload<RevokeSubscriptionInvocable, RevocationNotificationPayload>(payload);
            }

            return Ok();
        }

        /// <summary>
        /// Handle <see cref="StreamOnlineEvent"/> notifications from Twitch.
        /// </summary>
        [HttpPost]
        [RequireHeader(WebhookRequestHeaders.MessageType, WebhookRequestMessageTypes.Notification)]
        [RequireHeader(WebhookRequestHeaders.SubscriptionType, EventSubTypes.StreamOnline)]
        public async Task<IActionResult> PostEvent([FromBody] EventNotificationPayload<StreamOnlineEvent> payload)
            => await HandleEvent<StreamOnlineEvent, StreamOnlineInvocable>(payload);

        /// <summary>
        /// Handle event notifications from Twitch.
        /// </summary>
        /// <typeparam name="T">The event type</typeparam>
        /// <typeparam name="I">The invocable type</typeparam>
        private async Task<IActionResult> HandleEvent<T, I>(EventNotificationPayload<T> payload) where I : IInvocable, IInvocableWithPayload<EventNotificationPayload<T>>
        {
            VerifyMessageResult verifyResult = await VerifyMessageAsync();
            if (!verifyResult.Verified)
            {
                return verifyResult.ErrorActionResult ?? BadRequest();
            }

            if (!verifyResult.Duplicate)
            {
                _queue.QueueInvocableWithPayload<I, EventNotificationPayload<T>>(payload);
            }

            return Ok();
        }

        /// <summary>
        /// Record returned by <see cref="VerifyMessageAsync"/>.
        /// </summary>
        /// <param name="Verified">Whether the message is verified.</param>
        /// <param name="Duplicate">Whether the message is a duplicate. Will always be false if <paramref name="Verified"/> is false since it won't be checked.</param>
        /// <param name="ErrorActionResult">If <paramref name="Verified"/> is false, this should be returned by the endpoint that called <see cref="VerifyMessageAsync"/>. Otherwise it will be null.</param>
        private record VerifyMessageResult(bool Verified, bool Duplicate, IActionResult? ErrorActionResult);

        private async Task<VerifyMessageResult> VerifyMessageAsync()
        {
            IHeaderDictionary headers = HttpContext.Request.Headers;
            if (!headers.TryGetValue(WebhookRequestHeaders.MessageId, out StringValues messageIdHeader)
                || (string?)messageIdHeader is not string messageId
                || !headers.TryGetValue(WebhookRequestHeaders.MessageTimestamp, out StringValues messageTimestampHeader)
                || (string?)messageTimestampHeader is not string messageTimestamp
                || !headers.TryGetValue(WebhookRequestHeaders.MessageSignature, out StringValues messageSignatureHeader)
                || (string?)messageSignatureHeader is not string messageSignature)
            {
                return new(false, false, BadRequest("Missing required request headers."));
            }

            // Validate signature
            HttpContext.Request.Body.Seek(0, SeekOrigin.Begin);
            using (StreamReader reader = new(HttpContext.Request.Body))
            {
                string requestBody = await reader.ReadToEndAsync();
                bool messageVerified = Utilities.VerifyMessageSignature(
                    requestBody,
                    messageId,
                    messageTimestamp,
                    messageSignature,
                    _twitchOptions.WebhookSecret
                    );
                if (!messageVerified)
                {
                    _logger.LogError(
                        "Signature validation failed for Twitch webhook request from [{ip}]",
                        HttpContext.Connection.RemoteIpAddress
                        );
                    return new(false, false, Unauthorized("Message signature validation failed."));
                }
            }

            // Guard against replay attacks
            if (!DateTimeOffset.TryParse(messageTimestamp, out DateTimeOffset messageTime))
            {
                return new(false, false, BadRequest($"{WebhookRequestHeaders.MessageTimestamp} header value has invalid format."));
            }

            DateTimeOffset expirationCutoffTime = DateTimeOffset.Now.Subtract(_messageExpirationMinutes);
            if (messageTime < expirationCutoffTime)
            {
                return new(
                    false,
                    false,
                    BadRequest($"Message is no longer valid as it is older than {_messageExpirationMinutes.Minutes} minutes.")
                    );
            }

            // Check if message is a duplicate
            var cacheKey = _messageIdCacheKeyPrefix + messageIdHeader;
            if (await _cache.HasAsync(cacheKey))
            {
                return new(true, true, null);
            }
            _cache.Remember(cacheKey, () => true, _messageIdCacheExpirationHours);
            return new(true, false, null);
        }
    }
}
