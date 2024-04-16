using Asiago.Core.Twitch.EventSub;
using Asiago.Core.Twitch.EventSub.Models;
using Asiago.Data;
using Coravel.Invocable;
using Microsoft.EntityFrameworkCore;

namespace Asiago.Invocables.Twitch
{
    /// <summary>
    /// An <see cref="IInvocable"/> that handles <see cref="RevocationNotificationPayload"/> notifications from Twitch.
    /// </summary>
    internal class RevokeSubscriptionInvocable : IInvocable, IInvocableWithPayload<RevocationNotificationPayload>
    {
        public required RevocationNotificationPayload Payload { get; set; }

        private readonly ILogger<RevokeSubscriptionInvocable> _logger;
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public RevokeSubscriptionInvocable(
            ILoggerFactory loggerFactory,
            IDbContextFactory<ApplicationDbContext> dbContextFactory
            )
        {
            _logger = loggerFactory.CreateLogger<RevokeSubscriptionInvocable>();
            _dbContextFactory = dbContextFactory;
        }

        public async Task Invoke()
        {
            _logger.LogInformation(
                "Processing revocation notification for Twitch EventSub subscription [{id}][{type}] with reason [{reason}]",
                Payload.Subscription.Id,
                Payload.Subscription.Type,
                Payload.Reason
                );

            switch (Payload.Subscription.Type)
            {
                case EventSubTypes.StreamOnline:
                    await HandleStreamOnlineRevocation();
                    break;
                default:
                    _logger.LogError(
                        "Handling revocation notifications for Twitch EventSub subscriptions of type [{type}] is not implemented.",
                        Payload.Subscription.Type
                        );
                    break;
            }
        }

        private async Task HandleStreamOnlineRevocation()
        {
            if (Payload.Reason == RevocationReasons.UserRemoved)
            {
                using (var dbContext = _dbContextFactory.CreateDbContext())
                {
                    var twitchChannel = await dbContext.TwitchChannels
                        .SingleOrDefaultAsync(tc => tc.SubscriptionId == Payload.Subscription.Id);

                    if (twitchChannel == null)
                    {
                        _logger.LogWarning(
                            "Twitch channel subscription [{subscriptionId}] doesn't exist in the database. It may have already been deleted",
                            Payload.Subscription.Id
                            );
                        return;
                    }

                    dbContext.Remove(twitchChannel);

                    try
                    {
                        await dbContext.SaveChangesAsync();
                        _logger.LogInformation(
                            "Removed Twitch channel [{userId}] subscription [{subscriptionId}] from the database in response to revocation notification",
                            twitchChannel.UserId,
                            twitchChannel.SubscriptionId
                            );
                    }
                    catch (DbUpdateException ex)
                    {
                        _logger.LogWarning(
                            ex,
                            "Failed to remove Twitch channel [{userId}] from the database in response to revocation notification. It may have already been deleted",
                            twitchChannel.UserId
                            );
                    }
                }
            }
            else
            {
                _logger.LogError(
                    "Twitch EventSub subscription [{id}][{type}] has been revoked with reason [{reason}]. Bot owner intervention may be required.",
                    Payload.Subscription.Id,
                    Payload.Subscription.Type,
                    Payload.Reason
                    );
            }
        }
    }
}
