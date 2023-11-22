namespace Plugin.FirebasePushNotifications.Platforms
{
    public class PostNotificationsPermission : Permissions.BasePlatformPermission
    {
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions
        {
            get
            {
                return new[] 
                {
                    (global::Android.Manifest.Permission.PostNotifications, true) 
                };
            }
        }
    }
}