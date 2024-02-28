using FluentAssertions;
using Newtonsoft.Json;

namespace Plugin.FirebasePushNotifications.Tests.Model
{
    public class NotificationCategoryTests
    {
        [Fact]
        public void ShouldCreateNotificationCategory()
        {
            // Act
            const string categoryId = "category1";
            var actions = new NotificationAction[]
            {
                new NotificationAction("action1", "title1", NotificationActionType.Default),
                new NotificationAction("action2", "title2", NotificationActionType.Default),
            };
            var notficationCategory = new NotificationCategory(categoryId, actions);

            var test = JsonConvert.SerializeObject(notficationCategory);

            // Assert
            notficationCategory.CategoryId.Should().Be(categoryId);
            notficationCategory.Actions.Should().HaveCount(actions.Length);
        }

        [Fact]
        public void ShouldCreateNotificationCategory_FromJson()
        {
            // Act
            var notificationCategoryJson = "" +
                "{\"categoryId\":\"category1\"," +
                " \"actions\":[" +
                "   {\"Id\":\"action1\",\"Title\":\"title1\",\"Type\":0,\"Icon\":null}," +
                "   {\"Id\":\"action2\",\"Title\":\"title2\",\"Type\":0,\"Icon\":null}" +
                "  ]," +
                "\"type\":0}";

            var notficationCategory = JsonConvert.DeserializeObject<NotificationCategory>(notificationCategoryJson);

            // Assert
            notficationCategory.CategoryId.Should().Be("category1");
            notficationCategory.Actions.Should().HaveCount(2);

            var action1 = notficationCategory.Actions.ElementAt(0);
            action1.Should().BeEquivalentTo(new NotificationAction("action1", "title1", NotificationActionType.Default));
            
            var action2 = notficationCategory.Actions.ElementAt(1);
            action2.Should().BeEquivalentTo(new NotificationAction("action2", "title2", NotificationActionType.Default));
        }
    }
}
