using FluentAssertions;
using Plugin.FirebasePushNotifications.Extensions;

namespace Plugin.FirebasePushNotifications.Tests.Extensions
{
    public class DictionaryExtensionsTests
    {
        [Theory]
        [ClassData(typeof(DictionaryExtensionsTestData))]
        public void ShouldTryGetBool(IDictionary<string, object> dictionary, string key, bool expectedSuccess, bool expectedValue)
        {
            // Act
            var success = dictionary.TryGetBool(key, out var value);

            // Assert
            success.Should().Be(expectedSuccess);
            value.Should().Be(expectedValue);
        }

        internal class DictionaryExtensionsTestData : TheoryData<IDictionary<string, object>, string, bool, bool>
        {
            public DictionaryExtensionsTestData()
            {
                // Empty dictionary
                this.Add(new Dictionary<string, object>(), "key1", false, false);

                // False values
                {
                    var dictionary = new Dictionary<string, object>
                    {
                        { "key1", "False" },
                        { "key2", "false" },
                        { "key3", "0" },
                    };

                    this.Add(dictionary, "key1", true, false);
                    this.Add(dictionary, "key2", true, false);
                    this.Add(dictionary, "key3", true, false);
                }

                // True values
                {
                    var dictionary = new Dictionary<string, object>
                    {
                        { "key1", "True" },
                        { "key2", "true" },
                        { "key3", "1" },
                    };

                    this.Add(dictionary, "key1", true, true);
                    this.Add(dictionary, "key2", true, true);
                    this.Add(dictionary, "key3", true, true);
                }

            }
        }
    }

}
