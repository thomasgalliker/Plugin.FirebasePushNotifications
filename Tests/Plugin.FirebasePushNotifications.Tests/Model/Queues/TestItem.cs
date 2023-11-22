using System.Diagnostics;

namespace Plugin.FirebasePushNotifications.Tests.Model.Queues
{
    [DebuggerDisplay("TestItem: Id={this.Id}")]
    public class TestItem
    {
        public int Id { get; set; }
    }
}