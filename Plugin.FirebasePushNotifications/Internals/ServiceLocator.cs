namespace Plugin.FirebasePushNotifications.Internals
{
    /// <summary>
    /// ServiceLocator can be used to resolve services in places
    /// where no dependency injection is possible.
    /// WARNING: The use of ServiceLocator may leads to bad design.
    ///          Be very careful when using it!
    /// </summary>
    /// <remarks>
    /// See also: https://stackoverflow.com/a/73521158/3090156
    /// </remarks>
    public static class ServiceLocator
    {
        public static T GetService<T>() => Current.GetService<T>();

        public static IServiceProvider Current =>
#if WINDOWS10_0_17763_0_OR_GREATER
        MauiWinUIApplication.Current.Services;
#elif ANDROID
        MauiApplication.Current.Services;
#elif IOS || MACCATALYST
        MauiUIApplicationDelegate.Current.Services;
#else
            null;
#endif
    }
}