using System.Security.Cryptography;
using System.Text;

namespace Asiago.Core.Twitch.EventSub
{
    /// <summary>
    /// Utilities for interacting with Twitch EventSub notifications.
    /// </summary>
    internal static class Utilities
    {
        /// <summary>
        /// Verifies the HMAC signature of a message to ensure that it actually came from Twitch.
        /// </summary>
        public static async Task<bool> VerifyMessageAsync(HttpRequest request, string secret)
        {
            if (!request.Headers.TryGetValue(WebhookRequestHeaders.MessageId, out var messageId)
                || !request.Headers.TryGetValue(WebhookRequestHeaders.MessageTimestamp, out var messageTimestamp)
                || !request.Headers.TryGetValue(WebhookRequestHeaders.MessageSignature, out var messageSignature))
            {
                return false;
            }

            string requestBody;
            using (StreamReader reader = new(request.Body))
            {
                requestBody = await reader.ReadToEndAsync();
            }

            byte[] hash;
            using (HMACSHA256 hmac = new(Encoding.UTF8.GetBytes(secret)))
            {
                string message = messageId + messageTimestamp + requestBody;
                hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
            }
            string hmacSignature = "sha256=" + Convert.ToHexString(hash).ToLower();

            byte[] hmacSignatureBytes = Encoding.UTF8.GetBytes(hmacSignature);
            byte[] messageSignatureBytes = Encoding.UTF8.GetBytes(messageSignature.ToString());
            return CryptographicOperations.FixedTimeEquals(hmacSignatureBytes, messageSignatureBytes);
        }
    }
}
