namespace Plugin.FirebasePushNotifications
{
    internal static class Exceptions
    {
        internal static Exception NotImplementedInReferenceAssembly()
        {
            return new NotImplementedException(
                "This functionality is not implemented for the current platform. " +
                "You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.");
        }

        internal static FirebaseAppInitializationException FailedToInitializeFirebaseApp(Exception innerException = null)
        {
#if ANDROID
            return new FirebaseAppInitializationException(
                "FirebaseApp.InitializeApp failed with exception. " +
                "Make sure the google-services.json file is present and marked as GoogleServicesJson.",
                innerException);
#elif IOS
            return new FirebaseAppInitializationException(
                "Firebase.Core.App.Configure failed with exception. " +
                "Make sure the GoogleService-Info.plist file is present and marked as BundleResource.",
                innerException);
#endif
            return new FirebaseAppInitializationException(null, NotImplementedInReferenceAssembly());
        }
    }

    public class FirebaseAppInitializationException : Exception
    {
        public FirebaseAppInitializationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}