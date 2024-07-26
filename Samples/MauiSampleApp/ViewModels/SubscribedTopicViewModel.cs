using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace MauiSampleApp.ViewModels
{
    public class SubscribedTopicViewModel
    {
        public SubscribedTopicViewModel(string topic, Func<string, Task> unsubscribeFunc)
        {
            this.Topic = topic;
            this.UnsubscribeCommand = new AsyncRelayCommand(() => unsubscribeFunc(this.Topic));
        }

        public string Topic { get; }

        public ICommand UnsubscribeCommand { get; }
    }
}