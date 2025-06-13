# Plugin.FirebasePushNotifications

[![Version](https://img.shields.io/nuget/v/Plugin.FirebasePushNotifications.svg)](https://www.nuget.org/packages/Plugin.FirebasePushNotifications) [![Downloads](https://img.shields.io/nuget/dt/Plugin.FirebasePushNotifications.svg)](https://www.nuget.org/packages/Plugin.FirebasePushNotifications) [![Buy Me a Coffee](https://img.shields.io/badge/support-buy%20me%20a%20coffee-FFDD00)](https://buymeacoffee.com/thomasgalliker)

Plugin.FirebasePushNotifications provides a seamless way to engage users and keep them informed about important events
in your .NET MAUI applications. This open-source C# library integrates Firebase Cloud Messaging (FCM) into your .NET
MAUI projects, enabling you to receive push notifications effortlessly.

## Features

* Cross-platform Compatibility: Works seamlessly with .NET MAUI, ensuring a consistent push notification experience
  across different devices and platforms.
* Easy Integration: Simple setup process to incorporate Firebase Push Notifications into your .NET MAUI apps.
* Flexible Messaging: Utilize FCM's powerful features, such as targeted messaging, to send notifications based on user
  segments or specific conditions.

## Download and Install Plugin.FirebasePushNotifications

This library is available on NuGet: https://www.nuget.org/packages/Plugin.FirebasePushNotifications
Use the following command to install Plugin.FirebasePushNotifications using NuGet package manager console:

    PM> Install-Package Plugin.FirebasePushNotifications

You can use this library in any .NET MAUI project compatible to .NET 7 and higher.

## Setup

### Setup Firebase Push Notifications

- Go to https://console.firebase.google.com and create a new project. The setup of Firebase projects is not (yet?)
  documented here. Contributors welcome!
- You have to download the resulting Firebase service files and integrate them into your .NET MAUI csproj file.
  `google-services.json` is used by Android while `GoogleService-Info.plist` is accessible to iOS. Make sure the Include
  and the Link paths match.

```
<ItemGroup Condition="$(TargetFramework.Contains('-android'))">
	<GoogleServicesJson Include="Platforms\Android\Resources\google-services.json" Link="Platforms\Android\Resources\google-services.json" />
</ItemGroup>

<ItemGroup Condition="$(TargetFramework.Contains('-ios'))">
	<BundleResource Include="Platforms\iOS\GoogleService-Info.plist" Link="GoogleService-Info.plist" />
</ItemGroup>
```

- iOS apps need to be enabled to support push notifications. Turn on the "Push Notifications" capability of your app in
  the [Apple Developer Portal](https://developer.apple.com).

### MAUI App Startup

This plugin provides an extension method for MauiAppBuilder `UseFirebasePushNotifications` which ensure proper startup
and initialization. Call this method within your `MauiProgram` just as demonstrated in the MauiSampleApp:

```csharp
var builder = MauiApp.CreateBuilder()
    .UseMauiApp<App>()
    .UseFirebasePushNotifications();
```

`UseFirebasePushNotifications` has optional configuration parameters which are documented in another section of this
document.

### Android-specific Setup

- Copy the google-services.json to path location Platforms\Android\Resources\google-services.json (depending on what is
  configured in the csproj file).
- Make sure your launcher activity (usually this is MainActivity - but not always) uses
  `LaunchMode = LaunchMode.SingleTask`. You can also use a different LaunchMode; just be very sure what you do!

### iOS-specific Setup

- Copy the GoogleService-Info.plist to path location Platforms\iOS\GoogleService-Info.plist (depending on what is
  configured in the csproj file).
- Extend the AppDelegate.cs file with following method exports:

```csharp
[Export("application:didRegisterForRemoteNotificationsWithDeviceToken:")]
[BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
public void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
{
    IFirebasePushNotification.Current.RegisteredForRemoteNotifications(deviceToken);
}

[Export("application:didFailToRegisterForRemoteNotificationsWithError:")]
[BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
public void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
{
    IFirebasePushNotification.Current.FailedToRegisterForRemoteNotifications(error);
}

[Export("application:didReceiveRemoteNotification:fetchCompletionHandler:")]
public void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
{
    IFirebasePushNotification.Current.DidReceiveRemoteNotification(userInfo);
    completionHandler(UIBackgroundFetchResult.NewData);
}
```

## API Usage

`IFirebasePushNotification` is the main interface which handles most of the desired Firebase push notification features.
This interface is injectable via dependency injection or accessible as a static singleton instance
`IFirebasePushNotification.Current`. We strongly encourage you to use the dependency injection approach in order to keep
your code testable.

The following lines of code demonstrate how the `IFirebasePushNotification` instance is injected in `MainViewModel` and
assigned to a local field for later use:

```csharp
public MainViewModel(
    ILogger<MainViewModel> logger,
    IFirebasePushNotification firebasePushNotification)
{
    this.logger = logger;
    this.firebasePushNotification = firebasePushNotification;
}
```

### Notification Permissions

Before we can receive any notification we need to make sure the user has given consent to receive notifications.
`INotificationPermissions` is the service you can use to check the current authorization status or ask for notification
permission.
You can either inject `INotificationPermissions` into your view models or access it via the the static singleton
instance `INotificationPermissions.Current`.

- Check the current notification permission status:

```csharp
this.AuthorizationStatus = await this.notificationPermissions.GetAuthorizationStatusAsync();
```

- Ask the user for notification permission:

```csharp
await this.notificationPermissions.RequestPermissionAsync();
```

Notification permissions are handled by the underlying operating system (iOS, Android). This library just wraps the
platform-specific methods and provides a uniform API for them.

### Register for Notifications

The main goal of a push notification client library is to receive notification messages. This library provides a set of
classic .NET events to inform your code about incoming push notifications.
Before any notification event is received, we have to inform the Firebase client library, that we're ready to receive
notifications.
`RegisterForPushNotificationsAsync` registers our app with the Firebase push notification backend and receives a token.
This token is used by your own server/backend to send push notifications directly to this particular app instance.
The token may change after some time. It is not controllable by this library if/when the token is going to be updated.
The `TokenRefreshed` event will be fired whenever a new token is available.
See `Token` property and `TokenRefreshed` event provided by `IFirebasePushNotification` for more info.

```csharp
await this.firebasePushNotification.RegisterForPushNotificationsAsync();
```

If we want to turn off any incoming notifications, we can unregister from push notifications. The `Token` can no longer
be used to send push notifications to.

```csharp
await this.firebasePushNotification.UnregisterForPushNotificationsAsync();
```

### Receive Notifications
Following .NET events can be subscribed.

| Events                 | Description                                                                                                                                                                                                                                          |
|------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `TokenRefreshed`       | Is raised whenever the Firebase push notification token is updated. You'll need to inform your server/backend whenever a new push notification token is available.                                                                                   |
| `NotificationReceived` | Is raised when a new push notification message was received.                                                                                                                                                                                         |
| `NotificationOpened`   | Is raised when a received push notification is opened. This means, a user taps on a received notification listed in the notification center provided by the OS.                                                                                      |
| `NotificationAction`   | Is raised when the user taps a notification action. Notification actions allow users to make simple decisions when a notification is received, e.g. "Do you like to take your medicine?" could be answered with "Take medicine" and "Skip medicine". |
| `NotificationDeleted`  | Is raised when the user deletes a received notification.                                                                                                                                                                                             |

#### Notification Handling Behavior
The following table documents the behavior of each platform for incoming push notifications. We distinguish between notification message and data message.
- **Notification messages** must have a `notification` part with `title` and/or `body`. Other sections such as `data` are optional.
- **Data messages** must only have a `data` section (and can have platform specific options). The purpose of this message type is to send data-only messages without displaying a system notification popup.

The behavior when receiving notifications is also different if the app runs in foreground or in background mode.
The following table illustrates some use cases with different message types, priority flags and app states.

| Message Type         | Data Payload       | OS      | App State  | Notification Channel | Behavior                                                                    |
|----------------------|--------------------|---------|------------|----------------------|-----------------------------------------------------------------------------|
| Notification message | -                  | Android | Foreground |                      | `NotificationReceived` event is fired                                       |
| Notification message | -                  | iOS     | Foreground |                      | `NotificationReceived` event is fired                                       |
| Notification message | `priority: "low"`  | Android | Foreground |                      | `NotificationReceived` event is fired                                       |
| Notification message | `priority: "low"`  | iOS     | Foreground |                      | `NotificationReceived` event is fired                                       |
| Notification message | `priority: "high"` | Android | Foreground | `Importance=Default` | `NotificationReceived` event is fired + Notification icon in status bar     |
| Notification message | `priority: "high"` | Android | Foreground | `Importance=High`    | `NotificationReceived` event is fired + System notification popup           |
| Notification message | `priority: "high"` | iOS     | Foreground |                      | `NotificationReceived` event is fired + System notification popup           |
| Notification message | -                  | Android | Background | `Importance=Default` | Notification icon in status bar                                             |
| Notification message | -                  | Android | Background | `Importance=High`    | System notification popup                                                   |
| Notification message | -                  | iOS     | Background |                      | System notification popup                                                   |
| Notification message | `priority: "low"`  | Android | Background | `Importance=Default` | Notification icon in status bar                                             |
| Notification message | `priority: "low"`  | Android | Background | `Importance=High`    | System notification popup                                                   |
| Notification message | `priority: "low"`  | iOS     | Background |                      | System notification popup                                                   |
| Notification message | `priority: "high"` | Android | Background | `Importance=Default` | Notification icon in status bar                                             |
| Notification message | `priority: "high"` | Android | Background | `Importance=High`    | System notification popup                                                   |
| Notification message | `priority: "high"` | iOS     | Background |                      | System notification popup                                                   |
|                      |                    |         |            |                      |                                                                             |
| Data message         |                    | Android | Foreground |                      | `NotificationReceived` event is fired                                       |
| Data message         |                    | iOS     | Foreground |                      | `NotificationReceived` event is fired                                       |
| Data message         |                    | Android | Background |                      | `NotificationReceived` event is fired as soon as app enters foreground mode |
| Data message         |                    | iOS     | Background |                      | `NotificationReceived` event is fired as soon as app enters foreground mode |

*) _System notification popup: Official name is **heads-up notification** on Android and **banner notification** on
iOS._

*) _Notification channels exist on Android since Android 8.0 (API level 26)._

### Topics

The most common way of sending push notifications is by targeting notification message directly to push tokens.
Firebase allows to send push notifications to groups of devices, so-called topics.
If a user subscribes to a topic, e.g. "weather_updates" you can send push notifications to this topic instead of a list
of push tokens.

#### Subscribe to Topic

Use method `SubscribeTopicAsync` with the name of the topic.

```csharp
this.firebasePushNotification.SubscribeTopicAsync("weather_updates");
```

> [!IMPORTANT]
> - Make sure you did run `RegisterForPushNotificationsAsync` before you subscribe to topics.
> - Topic names are case-sensitive: Registrations for topic `"weather_updates"` will not receive messages targeted to topic `"Weather_Updates"`.

#### Send Notifications to Topic Subscribers

Use the Firebase Admin SDK (or any other HTTP client) to send a push notification targeting subscribers of the topic `"weather_updates"`. 
Instead of message property `to` which addresses an FCM token directly, we use `topic` to send notification messages to a whole group of subscribed devices.

`HTTP POST https://fcm.googleapis.com/v1/projects/{{project_id}}/messages:send`

```
{
    "message": {
        "topic": "weather_updates",
        "notification": {
            "title": "Weather Update",
            "body": "Pleasant with clouds and sun"
        },
        "data": {
            "sunrise": "1684926645",
            "sunset": "1684977332",
            "temp": "292.55",
            "feels_like": "292.87"
        }
    }
}
```

![Notification Topic weather_updates](Docs/notificationtopic_weatherupdates.png?raw=true)

### Notification Actions

Notification actions are special buttons which allow for immediate response to a particular notification. A list of
`NotificationActions` is consolidated within a `NotificationCategory`.

#### Register Notification Actions

The following example demonstrates the registration of a notification category with identifier "medication_intake" and
two actions "Take medicine" and "Skip medicine":

```csharp
var categories = new[]
{
    new NotificationCategory("medication_intake", new[]
    {
        new NotificationAction("take_medication", "Take medicine", NotificationActionType.Foreground),
        new NotificationAction("skip_medication", "Skip medicine", NotificationActionType.Foreground),
    })
};
```

Notification categories are usually registered at app startup time using the following method call:

```csharp
IFirebasePushNotification.Current.RegisterNotificationCategories(categories);
```

#### Subscribe to Notification Actions

Subscribe the event `IFirebasePushNotification.NotificationAction` to get notified if a user presses one of the
notification action buttons.
The delivered event args `FirebasePushNotificationResponseEventArgs` will let you know which action was pressed.

#### Send Notification Actions

Use the Firebase Admin SDK (or any other HTTP client) to send a push notification with:

`HTTP POST https://fcm.googleapis.com/v1/projects/myproject-b5ae1/messages:send`

```
{
    "message": {
        "token": "XXXXXXXXXX",
        "notification": {
            "title": "Medication Intake",
            "body": "Do you want to take your medicine?"
        },
        "data": {
            "priority": "high"
        },
        "android": {
            "notification": {
                "click_action": "medication_intake"
            }
        },
        "apns": {
            "payload": {
                "aps": {
                    "category": "medication_intake"
                }
            }
        }
    }
}
```

If everything works fine, the mobile device with the given token displays the notification action as follows:

![Notification Category medication_intake](Docs/notificationcategory_takemedicine.png?raw=true)

### Notification Channels

Notification channels are an Android feature introduced in Android 8.0 (API level 26) that let users manage notification
settings for different categories of notifications within the app.
Each notification channel represents a distinct type of notification (such as chat messages, medication intake, or
promotions) and allows users to customize notification preferences per channel, rather than for the whole app.

#### Default Notification Channel

Your app must always have at least one notification channel. This library will use this channel for any notifications
that are not targeting a specific channel. This ensures that push notifications are delivered even if a custom channel
is not set up.

It is highly recommended to create the default notification channel by yourself so that all properties are under your
control.
Use `INotificationChannels.Channels` methods to create notification channels manually at startup or specify them in the
Android-specific options under `UseFirebasePushNotifications(o => o.Android.NotificationChannels = ...)`).
To get an idea of how to use the `NotificationChannels` option, take a look at `MauiProgram.cs` in the sample app in
this repository.

#### Notification Channel Importance

When your app (or this library) creates a notification channel, you must specify its importance (e.g., `Low`, `Default`,
`High`).
Importance controls how notifications are presented (such as whether they make a sound or appear as a heads-up
notification).

> [!IMPORTANT]
> The importance level can only be set once when the channel is created. It cannot be changed afterward.
> If you need to modify a channel’s importance, you must create a new channel with a different ID.

#### Notification Channel Groups

Multiple notification channels may be grouped together within a notification channel group.
Use `INotificationChannels.ChannelGroups` methods to create/delete notification channel groups.

### More Push Notification Scenarios

There are a lot of features in this library that can be controlled via specific data flags. The most common scenarios
are end-to-end tested with the MauiSampleApp using postman calls. You can find an
up-to-date [postman collection](<Docs/FCM Plugin.FirebasePushNotifications.postman_collection.json>) in this repository.

- Import the collection in postman.
- Adjust the variables, especially the `project_id` and the `fcm_token` accordingly.
- Get a Bearer authentication token either by selecting the Auth Type "Firebase Cloud Messaging API (Oauth 2.0)" or by
  creating it manually via https://developers.google.com/oauthplayground (see
  this [youtube video](https://www.youtube.com/watch?v=PYfpBwupoMQ)).

### Options

> *to be documented*

## Contribution

If you find a bug or want to propose a new feature, feel free to create a new
issue [here](https://github.com/thomasgalliker/Plugin.FirebasePushNotifications/issues/new/choose).
Please use the **predefined issue templates** when submitting a new issue.

## Thank You

Your contribution is valuable!
Open source software isn’t just something you can pick up for free — it represents the hard work and dedication of many
people who often not even know each other.
We sincerely appreciate the time, effort, and dedication shown by everyone who helps keep this plugin going forward.

## Links

- FCM messages, data format, concepts and options:

  https://firebase.google.com/docs/cloud-messaging/concept-options

- Set up a Firebase Cloud Messaging client app on Apple platforms:

  https://firebase.google.com/docs/cloud-messaging/ios/client

- Set up a Firebase Cloud Messaging client app on Android:

  https://firebase.google.com/docs/cloud-messaging/android/client

- Expandable notification on Android:

  https://developer.android.com/develop/ui/views/notifications/expanded

- Create bearer authentication tokens for Firebase Cloud Messaging (and other Google APIs):

  https://developers.google.com/oauthplayground