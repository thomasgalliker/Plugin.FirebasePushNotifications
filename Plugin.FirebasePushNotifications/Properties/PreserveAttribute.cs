using System.ComponentModel;

namespace Plugin.FirebasePushNotifications
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal sealed class PreserveAttribute : Attribute
    {
        public bool AllMembers;
        public bool Conditional;

        public PreserveAttribute()
        {
        }

        public PreserveAttribute(Type type)
        {
        }
    }
}


