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

#### Receive Notification Events
> *to be documented*

#### Topics
> *to be documented*

#### Notification Actions 
> *to be documented*

### Options
> *to be documented*

### Contribution
Contributors welcome! If you find a bug or you want to propose a new feature, feel free to do so by opening a new issue on github.com.
