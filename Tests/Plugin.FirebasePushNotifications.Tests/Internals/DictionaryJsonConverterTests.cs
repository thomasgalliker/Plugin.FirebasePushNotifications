using FluentAssertions;
using Plugin.FirebasePushNotifications.Internals;
using Xunit.Abstractions;

namespace Plugin.FirebasePushNotifications.Tests.Internals
{
    public class DictionaryJsonConverterTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public DictionaryJsonConverterTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void ShouldFlattenObjectToDictionary()
        {
            // Arrange
            var source = new TestObject
            {
                Title = "Title",
                Date = new DateTime(2000, 1, 1, 12, 00, 00, DateTimeKind.Utc),
                NestedTestObject = new NestedTestObject
                {
                    Title = "NestedTitle",
                },
                TestArray = new[] { "value1", "value2" }
            };

            // Act
            var json = DictionaryJsonConverter.Flatten(source);

            // Assert
            this.testOutputHelper.WriteLine(ObjectDumper.Dump(json, DumpStyle.CSharp));

            json.Should().NotBeNull();
            json.Should().BeEquivalentTo(
                new Dictionary<string, string>
                {
                    { "Title", "Title" },
                    { "Date", "2000-01-01T12:00:00Z" },
                    { "NestedTestObject.Title", "NestedTitle" },
                    { "TestArray[0]", "value1" },
                    { "TestArray[1]", "value2" }
                });
        }

        [Fact]
        public void ShouldUnflattenDictionaryToObject()
        {
            // Arrange
            var source = new Dictionary<string, string>
            {
                { "Title", "Title" },
                { "Date", "2000-01-01T12:00:00Z" },
                { "NestedTestObject.Title", "NestedTitle" },
                { "TestArray[0]", "value1" },
                { "TestArray[1]", "value2" }
            };

            // Act
            var target = DictionaryJsonConverter.Unflatten<TestObject>(source);

            // Assert
            this.testOutputHelper.WriteLine(ObjectDumper.Dump(target, DumpStyle.CSharp));

            target.Should().NotBeNull();
            target.Should().BeOfType<TestObject>();
            target.Should().BeEquivalentTo(new TestObject
            {
                Title = "Title",
                Date = new DateTime(2000, 1, 1, 12, 00, 00, DateTimeKind.Utc),
                NestedTestObject = new NestedTestObject
                {
                    Title = "NestedTitle"
                },
                TestArray = new[] { "value1", "value2" }
            });
        }

        public class TestObject
        {
            public string Title { get; set; }

            public DateTime Date { get; set; }

            public NestedTestObject NestedTestObject { get; set; }

            public string[] TestArray { get; set; }
        }

        public class NestedTestObject
        {
            public string Title { get; set; }
        }
    }
}
