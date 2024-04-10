using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace MauiSampleApp.ViewModels
{
    public class SubscribedTopicViewModel
    {
        public SubscribedTopicViewModel(string topic, Func<string, Task> unsubscribeCommand)
        {
            this.Topic = topic;
            this.UnsubscribeCommand = new AsyncRelayCommand(() => unsubscribeCommand(this.Topic));
        }

        public string Topic { get; }

        public ICommand UnsubscribeCommand { get; set; }
    }
}