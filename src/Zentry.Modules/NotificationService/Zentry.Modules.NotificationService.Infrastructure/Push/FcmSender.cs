using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
using Zentry.Modules.NotificationService.Infrastructure.DeviceTokens;
using Zentry.Modules.NotificationService.Infrastructure.Persistence;

namespace Zentry.Modules.NotificationService.Infrastructure.Push;

/// <summary>
/// Gửi push notification sử dụng Firebase Cloud Messaging.
/// </summary>
public class FcmSender : IFcmSender
{
    private readonly ILogger<FcmSender> _logger;
    private readonly IDeviceTokenRepository _deviceTokenRepository; // Sẽ được tạo sau

    public FcmSender(ILogger<FcmSender> logger, IDeviceTokenRepository deviceTokenRepository)
    {
        _logger = logger;
        _deviceTokenRepository = deviceTokenRepository;

        // Khởi tạo Firebase Admin SDK.
        // **QUAN TRỌNG**: Bạn cần cung cấp file credentials của mình.
        // if (FirebaseApp.DefaultInstance == null)
        // {
        //     FirebaseApp.Create(new AppOptions
        //     {
        //         Credential = GoogleCredential.FromFile("path/to/your/firebase-credentials.json")
        //     });
        // }
    }

    public async Task SendPushNotificationAsync(Guid recipientUserId, string title, string body, IReadOnlyDictionary<string, string>? data, CancellationToken cancellationToken)
    {
        var fcmTokens = await _deviceTokenRepository.GetTokensByUserIdAsync(recipientUserId, cancellationToken);
        if (!fcmTokens.Any())
        {
            _logger.LogWarning("No FCM tokens found for user {UserId}. Skipping push notification.", recipientUserId);
            return;
        }

        var message = new MulticastMessage
        {
            Tokens = fcmTokens,
            Notification = new Notification
            {
                Title = title,
                Body = body
            },
            Data = data,
            Apns = new ApnsConfig
            {
                Aps = new Aps
                {
                    Sound = "default"
                }
            }
        };

        try
        {
            var response = await FirebaseMessaging.DefaultInstance.SendMulticastAsync(message, cancellationToken);
            if (response.FailureCount > 0)
            {
                var failedTokens = new List<string>();
                for (var i = 0; i < response.Responses.Count; i++)
                {
                    if (!response.Responses[i].IsSuccess)
                    {
                        failedTokens.Add(fcmTokens[i]);
                    }
                }
                _logger.LogError("Failed to send push notification to {FailureCount} tokens for user {UserId}. Failed tokens: {FailedTokens}",
                    response.FailureCount, recipientUserId, string.Join(", ", failedTokens));
                
                // Optional: Xóa các token không hợp lệ khỏi DB
                await _deviceTokenRepository.RemoveTokensAsync(failedTokens, cancellationToken);
            }

            _logger.LogInformation("Successfully sent push notification to {SuccessCount} tokens for user {UserId}.",
                response.SuccessCount, recipientUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while sending push notification for user {UserId}", recipientUserId);
            throw;
        }
    }
}

// Interface và class giả để biên dịch được.
// Interface moved to separate file: Infrastructure/DeviceTokens/DeviceTokenRepository.cs
// Implementation also moved to integrate with DeviceManagement module 