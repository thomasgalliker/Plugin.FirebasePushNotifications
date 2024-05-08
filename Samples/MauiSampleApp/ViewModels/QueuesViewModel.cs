using CommunityToolkit.Mvvm.Input;
using MauiSampleApp.Services;
using Plugin.FirebasePushNotifications;
using Plugin.FirebasePushNotifications.Model.Queues;
using System.Windows.Input;

namespace MauiSampleApp.ViewModels
{
    public class QueuesViewModel
    {
        private readonly IQueue<FirebasePushNotificationDataEventArgs> testQueue;
        private readonly IDialogService dialogService;

        private ICommand enqueueCommand;
        private ICommand tryDequeueAllCommand;

        public QueuesViewModel(IDialogService dialogService)
        {
            this.dialogService = dialogService;

            this.testQueue = new PersistentQueue<FirebasePushNotificationDataEventArgs>("testQueue");
        }

        public ICommand EnqueueCommand => this.enqueueCommand ??= new RelayCommand(this.Enqueue);

        private void Enqueue()
        {
            var dict = new Dictionary<string, object>
            {
                { "title", "Title" },
                { "body", "Body" },
                { "date", DateTime.Now.ToString("G") },
            };
            this.testQueue.Enqueue(new FirebasePushNotificationDataEventArgs(dict));
        }

        public ICommand TryDequeueAllCommand => this.tryDequeueAllCommand ??= new RelayCommand(this.TryDequeueAll);

        private void TryDequeueAll()
        {
            var count = this.testQueue.Count;

            var items = this.testQueue.TryDequeueAll();
            var itemsString = 
                $"queue.Count={count}{Environment.NewLine}{Environment.NewLine}" +
                string.Join(Environment.NewLine, items.Select(i => $"{i.GetType().Name}:{Environment.NewLine}" +
                $"{string.Join(Environment.NewLine, i.Data.Select(d => $"{{{d.Key}={d.Value}}}"))}" +
                $"{Environment.NewLine}"));

            _ = this.dialogService.ShowDialogAsync("TryDequeueAll", itemsString, "OK");
        }
    }
}
