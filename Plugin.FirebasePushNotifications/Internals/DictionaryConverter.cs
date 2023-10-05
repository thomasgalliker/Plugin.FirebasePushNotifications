using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Plugin.FirebasePushNotifications.Internals
{
    /// <summary>
    /// Converts any object into a flat dictionary with string key and string value.
    /// All nested properties are flattened into a dot-separated structure.
    /// </summary>
    public class DictionaryConverter
    {
        /// <summary>
        /// Creates a flat dictionary for <paramref name="source"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="source">Source object.</param>
        /// <returns>Flat dictionary with path as key and value as string.</returns>
        public static IDictionary<string, string> Flatten<T>(T source)
        {
            var jObject = JObject.FromObject(source);
            return Flatten(jObject);
        }

        /// <summary>
        /// Creates a flat dictionary for <paramref name="source"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="source">Source object.</param>
        /// <param name="jsonSerializer">Custom JsonSerializer used to serialize <paramref name="source"/>.</param>
        /// <returns>Flat dictionary with path as key and value as string.</returns>
        public static IDictionary<string, string> Flatten<T>(T source, JsonSerializer jsonSerializer)
        {
            var jObject = JObject.FromObject(source, jsonSerializer);
            return Flatten(jObject);
        }

        /// <summary>
        /// Creates a flat dictionary for <paramref name="source"/> of type <seealso cref="JObject"/>.
        /// </summary>
        /// <param name="source">Source object.</param>
        /// <returns>Flat dictionary with path as key and value as string.</returns>
        public static IDictionary<string, string> Flatten(JObject source)
        {
            var jTokens = source
                .Descendants()
                .Where(p => !p.Any());

            var results = jTokens
                .Select(jToken =>
                {
                    var value = (jToken as JValue)?.Value?.ToString();
                    var key = jToken.Path;
                    return new KeyValuePair<string, string>(key, value);
                })
                .ToDictionary(x => x.Key, x => x.Value);

            return results;
        }

        /// <summary>
        /// Unflattens a source <paramref name="dictionary"/> into an target object of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Target object type.</typeparam>
        /// <param name="dictionary">Source dictionary.</param>
        /// <returns>Target object.</returns>
        public static T Unflatten<T>(IDictionary<string, string> dictionary)
        {
            var jObject = Unflatten(dictionary);
            return jObject.ToObject<T>();
        }

        /// <inheritdoc cref="Unflatten{T}(IDictionary{string, string})"/>
        /// <param name="jsonSerializer">A custom json serializer.</param>
        public static T Unflatten<T>(IDictionary<string, string> dictionary, JsonSerializer jsonSerializer)
        {
            var jObject = Unflatten(dictionary);
            return jObject.ToObject<T>(jsonSerializer);
        }

        public static object Unflatten(IDictionary<string, string> dictionary, Type targetType, JsonSerializer jsonSerializer)
        {
            var jObject = Unflatten(dictionary);
            return jObject.ToObject(targetType, jsonSerializer);
        }

        /// <summary>
        ///  Creates a JObject by a given flat dictionary.
        /// </summary>
        /// <param name="dictionary">Flat dictionary.</param>
        /// <returns> Hierarchical JObject.</returns>
        public static JObject Unflatten(IDictionary<string, string> dictionary)
        {
            JContainer result = null;
            var setting = new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Merge };
            foreach (var pathValue in dictionary)
            {
                if (result == null)
                {
                    result = UnflattenSingle(pathValue);
                }
                else
                {
                    result.Merge(UnflattenSingle(pathValue), setting);
                }
            }

            return result as JObject;
        }

        private static JContainer UnflattenSingle(KeyValuePair<string, string> keyValue)
        {
            var path = keyValue.Key;
            var value = keyValue.Value;
            var pathSegments = SplitPath(path);

            JContainer lastItem = null;
            
            // Build from leaf to root
            foreach (var pathSegment in pathSegments.Reverse())
            {
                var type = GetJsonTokenType(pathSegment);
                switch (type)
                {
                    case JTokenType.Object:
                        var obj = new JObject();
                        if (null == lastItem)
                        {
                            obj.Add(pathSegment, value);
                        }
                        else
                        {
                            obj.Add(pathSegment, lastItem);
                        }

                        lastItem = obj;
                        break;
                    case JTokenType.Array:
                        var array = new JArray();
                        var index = GetArrayIndex(pathSegment);
                        array = FillEmpty(array, index);
                        if (lastItem == null)
                        {
                            array[index] = value;
                        }
                        else
                        {
                            array[index] = lastItem;
                        }

                        lastItem = array;
                        break;
                    default:
                        throw new NotSupportedException($"UnflattenSingle does not support type {type}.");
                }
            }

            return lastItem;
        }

        public static IEnumerable<string> SplitPath(string path)
        {
            var reg = new Regex(@"(?!\.)([^. ^\[\]]+)|(?!\[)(\d+)(?=\])");
            foreach (Match match in reg.Matches(path))
            {
                yield return match.Value;
            }
        }

        private static JArray FillEmpty(JArray array, int index)
        {
            for (var i = 0; i <= index; i++)
            {
                array.Add(null);
            }

            return array;
        }

        private static JTokenType GetJsonTokenType(string pathSegment)
        {
            return int.TryParse(pathSegment, out _) ? JTokenType.Array : JTokenType.Object;
        }

        private static int GetArrayIndex(string pathSegment)
        {
            if (int.TryParse(pathSegment, out var result))
            {
                return result;
            }

            throw new Exception("Unable to parse array index: " + pathSegment);
        }
    }
}