using Foundation;
using ObjCRuntime;
using Plugin.FirebasePushNotifications;
using UIKit;

namespace MauiSampleApp
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp()
        {
            return MauiProgram.CreateMauiApp();
        }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            return base.FinishedLaunching(application, launchOptions);
        }

        [Export("application:didRegisterForRemoteNotificationsWithDeviceToken:")]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
        {
            CrossFirebasePushNotification.Current.RegisteredForRemoteNotifications(deviceToken);
        }

        [Export("application:didFailToRegisterForRemoteNotificationsWithError:")]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
        {
            CrossFirebasePushNotification.Current.FailedToRegisterForRemoteNotifications(error);
        }

        [Export("application:didReceiveRemoteNotification:fetchCompletionHandler:")]
        public void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
        {
            CrossFirebasePushNotification.Current.DidReceiveRemoteNotification(userInfo);
            completionHandler(UIBackgroundFetchResult.NewData);
        }
    }
}