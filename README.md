# Plugin.FirebasePushNotifications
[![Version](https://img.shields.io/nuget/v/Plugin.FirebasePushNotifications.svg)](https://www.nuget.org/packages/Plugin.FirebasePushNotifications)  [![Downloads](https://img.shields.io/nuget/dt/Plugin.FirebasePushNotifications.svg)](https://www.nuget.org/packages/Plugin.FirebasePushNotifications)

Plugin.FirebasePushNotifications provides a seamless way to engage users and keep them informed about important events in your .NET MAUI applications. This open-source C# library integrates Firebase Cloud Messaging (FCM) into your .NET MAUI projects, enabling you to receive push notifications effortlessly.

### Features
* Cross-platform Compatibility: Works seamlessly with .NET MAUI, ensuring a consistent push notification experience across different devices and platforms.
* Easy Integration: Simple setup process to incorporate Firebase Push Notifications into your .NET MAUI apps.
* Flexible Messaging: Utilize FCM's powerful features, such as targeted messaging, to send notifications based on user segments or specific conditions.

### Download and Install Plugin.FirebasePushNotifications
This library is available on NuGet: https://www.nuget.org/packages/Plugin.FirebasePushNotifications
Use the following command to install Plugin.FirebasePushNotifications using NuGet package manager console:

    PM> Install-Package Plugin.FirebasePushNotifications

You can use this library in any .NET MAUI project compatible to .NET 7 and higher.

### Setup
#### Setup Firebase Push Notifications
- Go to https://console.firebase.google.com and create a new project. The setup of Firebase projects is not (yet?) documented here. Contributors welcome!
- You have to download the resulting Firebase service files and integrate them into your .NET MAUI csproj file. `google-services.json` is used by Android while `GoogleService-Info.plist` is accessible to iOS. Make sure the Include and the Link paths match.
```
<ItemGroup Condition="$(TargetFramework.Contains('-android'))">
	<GoogleServicesJson Include="Platforms\Android\Resources\google-services.json" Link="Platforms\Android\Resources\google-services.json" />
</ItemGroup>

<ItemGroup Condition="$(TargetFramework.Contains('-ios'))">
	<BundleResource Include="Platforms\iOS\GoogleService-Info.plist" Link="GoogleService-Info.plist" />
</ItemGroup>
```
- iOS apps need to be enabled to support push notifications. Turn on the "Push Notifications" capability of your app in the [Apple Developer Portal](https://developer.apple.com).

#### App Startup

This plugin provides an extension method for MauiAppBuilder `UseFirebasePushNotifications` which ensure proper startup and initialization. Call this method within your `MauiProgram` just as demonstrated in the MauiSampleApp:

```csharp
var builder = MauiApp.CreateBuilder()
    .UseMauiApp<App>()
    .UseFirebasePushNotifications();
```

`UseFirebasePushNotifications` has optional configuration parameters which are documented in another section of this document.

### API Usage
`IFirebasePushNotification` is the main interface which handles most of the desired Firebase push notification features. This interface is injectable via dependency injection or accessible as a static singleton instance `CrossFirebasePushNotification.Current`. We highly encourage you to use the dependency injection approach in order to keep your code testable.

The following lines of code demonstrate how the `IFirebasePushNotification` instance is injected in `MainViewModel` and assigned to a local field for later use:
```csharp
public MainViewModel(
    ILogger<MainViewModel> logger,
    IFirebasePushNotification firebasePushNotification)
{
    this.logger = logger;
    this.firebasePushNotification = firebasePushNotification;
}
```

#### Managing Notification Permissions
Before we can receive any notification we need to make sure the user has given consent to receive notifications. `INotificationPermissions` is the service you can use to check the current authorization status or ask for notification permission.

- Check the current notification permission status:
```csharp
this.AuthorizationStatus = await this.notificationPermissions.GetAuthorizationStatusAsync();
```

- Ask the user for notification permission:
```csharp
await this.notificationPermissions.RequestPermissionAsync();
```

Notification permissions are handled by the underlying operating system (iOS, Android). This library just wraps the platform-specific methods and provides a uniform API for them.

#### Receive Notifications
Now, the main goal of a push notification client library is to receive notification messages. This library provides a set of classic .NET events to inform your code about incoming push notifications.
Before any notification event is received, we have to inform the Firebase client library, that we're ready to receive notifications. `RegisterForPushNotificationsAsync` registers our app with the Firebase push notfication backend and receives a Token. This Token is used by your own server/backend to send push notifications directly to this particular app instance.
See `Token` property and `TokenRefreshed` event provided by `IFirebasePushNotification` for more info.

```csharp
await this.firebasePushNotification.RegisterForPushNotificationsAsync();
```

If we want to turn off any incoming notifications, we can unregister from push notifications. The `Token` can no longer be used to send push notifications to.
```csharp
await this.firebasePushNotification.UnregisterForPushNotificationsAsync();
```

Following .NET events can be subscribed:
- `IFirebasePushNotification.TokenRefreshed` is raised whenever the Firebase push notification token is updated. You'll need to inform your server/backend whenever a new push notification token is available.

- `IFirebasePushNotification.NotificationReceived` is raised when a new push notification message was received.


- `IFirebasePushNotification.NotificationOpened` is raised when a received push notification is opened. This means, a user taps on a received notification listed in the notification center provided by the OS.

- `IFirebasePushNotification.NotificationAction` is raised when the user taps a notification action. Notification actions allow users to make simple decisions when a notification is received, e.g. "Do you like to take your medicine?" could be answered with "Take medicine" and "Skip medicine".

- `IFirebasePushNotification.NotificationDeleted` is raised when the user deletes a received notification.

- `IFirebasePushNotification.NotificationError` is raised in some particular error cases. _(Will be removed in future releases). _

#### Topics
The most common way of sending push notifications is by targeting notification message directly to push tokens. Firebase allows to send push notifications to groups of devices, so called topics. If a user subscribes to a topic, e.g. "weather_updates" you can send push notifications to this topic instead of a list of push tokens.

Use method SubscribeTopic with the name of the topic:
```csharp
this.firebasePushNotification.SubscribeTopic("weather_updates");
```

Use the Firebase Admin SDK (or any other HTTP client) to send a push notification targeting subscribers of the "weather_updates" topic:

`HTTP POST https://fcm.googleapis.com/fcm/send`
```
{
    "data": {
        "body" : "body",
        "title": "title"
     },
     "priority": "high",
     "condition": "'weather_updates' in topics"
}
```

#### Notification Actions 
> *to be documented*

### Options
> *to be documented*

### Contribution
Contributors welcome! If you find a bug or you want to propose a new feature, feel free to do so by opening a new issue on github.com.
