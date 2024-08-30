using CommunityToolkit.Mvvm.Input;
using MauiSampleApp.Services;
using Microsoft.Extensions.Logging;
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

        public QueuesViewModel(
            IDialogService dialogService,
            ILoggerFactory loggerFactory)
        {
            this.dialogService = dialogService;

            var persistentQueueFactory = new PersistentQueueFactory();
            this.testQueue = persistentQueueFactory.Create<FirebasePushNotificationDataEventArgs>("testQueue", loggerFactory);
        }

        public ICommand EnqueueCommand
        {
            get => this.enqueueCommand ??= new RelayCommand(this.Enqueue);
        }

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

        public ICommand TryDequeueAllCommand
        {
            get => this.tryDequeueAllCommand ??= new RelayCommand(this.TryDequeueAll);
        }

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
