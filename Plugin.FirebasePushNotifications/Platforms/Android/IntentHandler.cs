using Android.App;
using Android.Content;
using Android.Util;

namespace Plugin.FirebasePushNotifications.Platforms
{
    internal static class IntentHandler
    {
        private const string Tag = nameof(IntentHandler);

        internal static void CheckAndProcessIntent(Activity activity, Intent intent)
        {
            if (intent == null)
            {
                return;
            }

            var extras = intent.GetExtras();
            Log.Debug(Tag, $"CheckAndProcessIntent: Flags={intent.Flags}, Extras={string.Join(System.Environment.NewLine, extras)}");

            var launchedFromHistory = intent.Flags.HasFlag(ActivityFlags.LaunchedFromHistory);
            if (launchedFromHistory)
            {
                // Don't process the intent if flag FLAG_ACTIVITY_LAUNCHED_FROM_HISTORY is present
                return;
            }

            if (intent.Extras != null &&
                intent.Extras.KeySet().Any())
            {
                // Don't process old/historic intents which are recycled for whatever reasons
                var intentAlreadyHandledKey = Constants.ExtraFirebaseProcessIntentHandled;
                if (!intent.GetBooleanExtra(intentAlreadyHandledKey, false))
                {
                    intent.PutExtra(intentAlreadyHandledKey, true);
                    Log.Debug(Tag, $"CheckAndProcessIntent: {intentAlreadyHandledKey} not present --> Process push notification");

                    FirebasePushNotificationManager.ProcessIntent(activity, intent, enableDelayedResponse: false);
                }
                else
                {
                    Log.Debug(Tag, $"CheckAndProcessIntent: {intentAlreadyHandledKey} is present --> Push notification already processed");
                }
            }
        }
    }
}
