using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Plugin.FirebasePushNotifications.Internals;
using Plugin.FirebasePushNotifications.Model;
using Xunit;

namespace Plugin.FirebasePushNotifications.Tests.Internals
{
    public class DictionaryJsonConverterTests
    {
        [Fact]
        public void ShouldFlattenObjectToDictionary()
        {
            // Arrange
            var source = new TestObject
            {
                Title = "Title",
                Body = "Body",
                NestedTestObject = new NestedTestObject
                {
                    Title = "NestedTitle",
                }
            };

            // Act
            var json = DictionaryJsonConverter.Flatten(source);

            // Assert
            json.Should().NotBeNull();
            json.Should().BeEquivalentTo(
                new Dictionary<string, string>
                {
                    { "Title", "Title" },
                    { "Body", "Body" },
                    { "NestedTestObject.Title", "NestedTitle" }
                });
        }

        [Fact]
        public void ShouldUnflattenDictionaryToObject()
        {
            // Arrange
            var source = new Dictionary<string, string>
            {
                { "Title", "Title" },
                { "Body", "Body" },
                { "NestedTestObject.Title", "NestedTitle" }
            };

            // Act
            var target = DictionaryJsonConverter.Unflatten<TestObject>(source);

            // Assert
            target.Should().NotBeNull();
            target.Should().BeOfType<TestObject>();
            target.Should().BeEquivalentTo(new TestObject
            {
                Title = "Title",
                Body = "Body",
                NestedTestObject = new NestedTestObject
                {
                    Title = "NestedTitle"
                }
            });
        }

        public class TestObject
        {
            public string Title { get; set; }

            public string Body { get; set; }

            public NestedTestObject NestedTestObject { get; set; }
        }

        public class NestedTestObject
        {
            public string Title { get; set; }
        }
    }
}
