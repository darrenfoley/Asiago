﻿using Asiago.Controllers.Attributes;
using Asiago.Core.Twitch;
using Asiago.Core.Twitch.EventSub;
using Asiago.Core.Twitch.EventSub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

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
        private readonly ILogger<TwitchController> _logger;
        private readonly TwitchOptions _twitchOptions;

        public TwitchController(ILoggerFactory loggerFactory, IOptions<TwitchOptions> twitchOptions)
        {
            _logger = loggerFactory.CreateLogger<TwitchController>();
            _twitchOptions = twitchOptions.Value;
        }

        /// <summary>
        /// Handle verification challenge requests from Twitch in order to prove we own the event handler endpoint.
        /// </summary>
        [HttpPost]
        [RequireHeader(WebhookRequestHeaders.MessageType, WebhookRequestMessageTypes.WebhookCallbackVerification)]
        public async Task<IActionResult> PostCallbackVerification([FromBody] VerificationNotificationPayload payload)
        {
            (bool verified, string errorMessage) = await VerifyMessageAsync();
            if (!verified)
            {
                return Unauthorized(errorMessage);
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
            (bool verified, string errorMessage) = await VerifyMessageAsync();
            if (!verified)
            {
                return Unauthorized(errorMessage);
            }

            // TODO: do something about the revocation

            return Ok();
        }

        /// <summary>
        /// Handle <see cref="StreamOnlineEvent"/> notifications from Twitch.
        /// </summary>
        [HttpPost]
        [RequireHeader(WebhookRequestHeaders.MessageType, WebhookRequestMessageTypes.Notification)]
        [RequireHeader(WebhookRequestHeaders.SubscriptionType, EventSubTypes.StreamOnline)]
        public async Task<IActionResult> PostEvent([FromBody] EventNotificationPayload<StreamOnlineEvent> payload)
            => await HandleEvent(payload);

        private async Task<(bool verified, string errorMessage)> VerifyMessageAsync()
        {
            bool verified = true;
            string errorMessage = "";
            HttpContext.Request.Body.Seek(0, SeekOrigin.Begin);
            bool messageVerified = await Utilities.VerifyMessageAsync(HttpContext.Request, _twitchOptions.WebhookSecret);
            if (!messageVerified)
            {
                _logger.LogError(
                    "Signature validation failed for Twitch webhook request from [{ip}]",
                    HttpContext.Connection.RemoteIpAddress
                    );
                verified = false;
                errorMessage = "Message signature validation failed!";
            }
            return (verified, errorMessage);
        }

        private async Task<IActionResult> HandleEvent<T>(EventNotificationPayload<T> payload)
        {
            (bool verified, string errorMessage) = await VerifyMessageAsync();
            if (!verified)
            {
                return Unauthorized(errorMessage);
            }

            // TODO: do something

            return Ok();
        }
    }
}