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
- Configure your app to use logging (Microsoft.Extensions.Logging) and check the log output for any messages related to Plugin.FirebasePushNotifications.

### Where have the platform-specific methods gone?
It happened in the past, that iOS- or Android-specific methods from cross-platform interfaces (e.g. IFirebasePushNotification) were missing after a nuget update, which results in build errors.
- Some code files in .NET MAUI are partial across platforms. You may look at the wrong part of the file.
- If you cannot find platform-specific code this may be related to .NET workload problems. Make sure you use the latest workloads `dotnet workload update`. If this does not help, try to repair the workloads `dotnet workload repair`.

### Long path issue on Windows 11
Issues with the Xamarin.Firebase.iOS.Core package can cause installation failures on Windows due to excessively long file paths, as mentioned in issue [17828](https://github.com/dotnet/maui/issues/17828) of the dotnet/maui repository. To fix this, you need to enable long paths in the registry settings and relocate your local NuGet cache. Additionally, keeping your project path as short as possible is recommended.

The root cause of the long path issue lies in the XCFramework format, which tends to generate long file names. Unfortunately, Visual Studio on Windows has inherent limitations when handling long file names, and there's nothing this plugin can do to resolve that. Any concerns should be directed to the [Visual Studio team](https://developercommunity.visualstudio.com/t/Allow-building-running-and-debugging-a/351628), though this might not yield immediate results.

It's worth noting that macOS systems do not experience this issue, as they can manage long file names without trouble. While the plugin can still be built in Visual Studio on Windows by running dotnet restore outside the IDE, the archiving process will most likely need to be performed on a Mac.

- **Update registry to support long paths:** Run the following PowerShell script with elevated privileges.
    ```
    New-ItemProperty `
        -Path "HKLM:\SYSTEM\CurrentControlSet\Control\FileSystem" `
        -Name "LongPathsEnabled" `
        -Value 1 `
        -PropertyType DWORD `
        -Force
    ```

- **Use shorts paths for source code:** 
Move the source code of this repository to a short path location on your local disk, e.g. `C:\src`

- **Change the output directory for your project**: You can use a short path like `C:\bld` as output directory.

- **Use short paths for Nuget cache:**
Create a folder named `C:\n`. Add an environment variable `NUGET_PACKAGES = C:\n`

- **Install package via CLI:**
Close Visual Studio. Start a new command line and navigate to the project folder root path. Run the command: `dotnet add package Plugin.FirebasePushNotifications`

- **Use Jetbrains Rider on macOS:** This may not be an option in all cases, and I'm well aware that this may cause further implications, but it's worth mentioning. Download here: [Jetbrains Rider](https://jetbrains.com/rider/).

### Why is there no GoogleService-Info.plist and google-services.json in the sample app?
Two reasons: The files contain trustworthy API keys and they would not be of any help for you.
If you want to try the sample app, you'll need to create your own Firebase service files in your own Firebase project.
Otherwise, you'd not be able to send any push notifications to the sample app.

### I added GoogleService-Info.plist and google-services.json, but they are not recognized?
- Mark the GoogleService-Info.plist as BundleResource and Link it properly (see readme.md and sample app).
- Mark the google-services.json as GoogleServicesJson and Link it properly (see readme.md and sample app)
- Clean the whole solution using clean.bat (Windows) or clean.sh (macOS).
- Do a full solution rebuild after you've added the Google service files.

