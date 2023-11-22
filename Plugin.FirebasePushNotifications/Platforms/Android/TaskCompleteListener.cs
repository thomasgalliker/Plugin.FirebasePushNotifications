using Android.Gms.Tasks;

namespace Plugin.FirebasePushNotifications.Platforms
{
    internal class TaskCompleteListener : Java.Lang.Object, IOnCompleteListener
    {
        private readonly TaskCompletionSource<Java.Lang.Object> tcs;

        public TaskCompleteListener(TaskCompletionSource<Java.Lang.Object> tcs)
        {
            this.tcs = tcs;
        }

        public void OnComplete(global::Android.Gms.Tasks.Task task)
        {
            if (task.IsCanceled)
            {
                this.tcs.SetCanceled();
            }
            else if (task.IsSuccessful)
            {
                this.tcs.SetResult(task.Result);
            }
            else
            {
                this.tcs.SetException(task.Exception);
            }
        }
    }
}
