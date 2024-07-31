# Plugin.FirebasePushNotifications FAQ

## General Questions

### What are Firebase Push Notifications?
Firebase Push Notifications are a service provided by Firebase Cloud Messaging (FCM) that allows you to send notifications to users across multiple platforms such as Android, iOS, and web applications.

### How do Firebase Push Notifications work?
Firebase Push Notifications work by sending messages from your server or Firebase console to your app users via Firebase Cloud Messaging. The notifications can be triggered based on various conditions like user behavior or data changes.

## Technical Questions

### What are the message types supported by Firebase Cloud Messaging?
Firebase supports two main types of messages:
- **Notification messages:** Automatically handled by the FCM SDK. These are displayed in the system tray when your app is not in the foreground.
- **Data messages:** Custom messages that must be handled by the app's code, regardless of the app's state.
- **Combined notification messages:** A combination of notification and data message.

### Can I send notification messages to multiple tokens using the HTTP V1 API?
Sending a message to multiple tokens is no longer supported.
The general recommendations is to send individual requests. For a large number of recipients use topics.

### Is there a limitation of how many requests can be sent to the HTTP V1 API?
There is a rate limiting active on almost all FCM APIs:
If you receive an HTTP 429 you may need to back-off and send your messages later again.

## Troubleshooting

### Why are my notifications not being delivered?
Common reasons for notification delivery issues include:
- Incorrect Firebase configuration: Wrong package name or bundle identifier. Wrong google-services.json or GoogleService-Info.plist file used.
- The app being killed or not running in the background.
- Notification permissions not configured in AndroidManifest and/or user not asked for giving consent.
- Notification events not subscribed.
- Network connectivity issues.

### How can I debug notification issues?
- Verify your Firebase configuration and ensure that the correct services files are being used.
- Check log output for any errors related to Plugin.FirebasePushNotifications.