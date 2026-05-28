using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Plugin.FirebasePushNotifications.Internals
{
    /// <summary>
    /// Converts any object into a flat dictionary with string key and string value.
    /// All nested properties are flattened into a dot-separated structure.
    /// </summary>
    public static class DictionaryJsonConverter
    {
        private static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
        };

        /// <summary>
        /// Creates a flat dictionary for <paramref name="source"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="source">Source object.</param>
        /// <returns>Flat dictionary with path as key and value as string.</returns>
        public static IDictionary<string, string> Flatten<T>(T source)
        {
            return Flatten(source, DefaultJsonSerializerOptions);
        }

        /// <summary>
        /// Creates a flat dictionary for <paramref name="source"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="source">Source object.</param>
        /// <param name="jsonSerializerOptions">Custom options used to serialize <paramref name="source"/>.</param>
        /// <returns>Flat dictionary with path as key and value as string.</returns>
        public static IDictionary<string, string> Flatten<T>(T source, JsonSerializerOptions jsonSerializerOptions)
        {
            var jsonObject = JsonSerializer.SerializeToNode(source, jsonSerializerOptions) as JsonObject ?? new JsonObject();
            return Flatten(jsonObject);
        }

        /// <summary>
        /// Creates a flat dictionary for <paramref name="source"/> of type <seealso cref="JsonObject"/>.
        /// </summary>
        /// <param name="source">Source object.</param>
        /// <returns>Flat dictionary with path as key and value as string.</returns>
        public static IDictionary<string, string> Flatten(JsonObject source)
        {
            var results = new Dictionary<string, string>();

            if (source == null)
            {
                return results;
            }

            FlattenInternal(source, null, results);
            return results;
        }

        /// <summary>
        /// Unflattens a source <paramref name="dictionary"/> into an object of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Target object type.</typeparam>
        /// <param name="dictionary">The source dictionary.</param>
        /// <returns>Target object.</returns>
        public static T Unflatten<T>(IDictionary<string, string> dictionary)
        {
            var jsonObject = Unflatten(dictionary);
            return jsonObject.Deserialize<T>(DefaultJsonSerializerOptions);
        }

        /// <inheritdoc cref="Unflatten{T}(IDictionary{string, string})"/>
        /// <param name="dictionary">The source dictionary.</param>
        /// <param name="jsonSerializerOptions">A custom json serializer options instance.</param>
        public static T Unflatten<T>(IDictionary<string, string> dictionary, JsonSerializerOptions jsonSerializerOptions)
        {
            return (T)Unflatten(dictionary, typeof(T), jsonSerializerOptions);
        }

        /// <summary>
        /// Unflattens a source <paramref name="dictionary"/> into an object of type <paramref name="targetType"/>.
        /// </summary>
        /// <param name="dictionary">The source dictionary.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="jsonSerializerOptions">A custom json serializer options instance.</param>
        public static object Unflatten(IDictionary<string, string> dictionary, Type targetType, JsonSerializerOptions jsonSerializerOptions)
        {
            var jsonObject = Unflatten(dictionary);
            return jsonObject.Deserialize(targetType, jsonSerializerOptions);
        }

        /// <summary>
        ///  Creates a <see cref="JsonObject"/> from a given flat <paramref name="dictionary"/>.
        /// </summary>
        /// <param name="dictionary">The source dictionary.</param>
        /// <returns>The structured result.</returns>
        public static JsonObject Unflatten(IDictionary<string, string> dictionary)
        {
            var root = new JsonObject();

            foreach (var pathValue in dictionary)
            {
                InsertPathValue(root, pathValue.Key, pathValue.Value);
            }

            return root;
        }

        public static IEnumerable<string> SplitPath(string path)
        {
            var reg = new Regex(@"(?!\.)([^. ^\[\]]+)|(?!\[)(\d+)(?=\])");
            foreach (Match match in reg.Matches(path))
            {
                yield return match.Value;
            }
        }

        private static void FlattenInternal(JsonNode node, string currentPath, IDictionary<string, string> results)
        {
            switch (node)
            {
                case JsonObject jsonObject:
                    foreach (var property in jsonObject)
                    {
                        var nextPath = string.IsNullOrEmpty(currentPath)
                            ? property.Key
                            : $"{currentPath}.{property.Key}";

                        FlattenInternal(property.Value, nextPath, results);
                    }

                    break;

                case JsonArray jsonArray:
                    for (var i = 0; i < jsonArray.Count; i++)
                    {
                        var nextPath = $"{currentPath}[{i}]";
                        FlattenInternal(jsonArray[i], nextPath, results);
                    }

                    break;

                case JsonValue jsonValue:
                    results[currentPath] = GetJsonValueAsString(jsonValue);
                    break;

                case null:
                    results[currentPath] = null;
                    break;
            }
        }

        private static string GetElementStringValue(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Null => null,
                _ => element.ToString(),
            };
        }

        private static string GetJsonValueAsString(JsonValue jsonValue)
        {
            if (jsonValue.TryGetValue<string>(out var stringValue))
            {
                return stringValue;
            }

            if (jsonValue.TryGetValue<JsonElement>(out var jsonElement))
            {
                return GetElementStringValue(jsonElement);
            }

            return jsonValue.ToJsonString().Trim('"');
        }

        private static void InsertPathValue(JsonObject root, string path, string value)
        {
            var pathSegments = SplitPath(path).ToArray();
            JsonNode current = root;

            for (var i = 0; i < pathSegments.Length; i++)
            {
                var segment = pathSegments[i];
                var isLast = i == pathSegments.Length - 1;
                var isArraySegment = int.TryParse(segment, out var arrayIndex);

                if (isArraySegment)
                {
                    var array = current as JsonArray;
                    if (array == null)
                    {
                        throw new NotSupportedException($"Path segment '{segment}' is an array index but current node is not an array.");
                    }

                    EnsureArraySize(array, arrayIndex);

                    if (isLast)
                    {
                        array[arrayIndex] = JsonValue.Create(value);
                    }
                    else
                    {
                        var nextSegmentIsArray = int.TryParse(pathSegments[i + 1], out _);
                        array[arrayIndex] ??= nextSegmentIsArray ? new JsonArray() : new JsonObject();
                        current = array[arrayIndex];
                    }
                }
                else
                {
                    var obj = current as JsonObject;
                    if (obj == null)
                    {
                        throw new NotSupportedException($"Path segment '{segment}' is an object key but current node is not an object.");
                    }

                    if (isLast)
                    {
                        obj[segment] = JsonValue.Create(value);
                    }
                    else
                    {
                        var nextSegmentIsArray = int.TryParse(pathSegments[i + 1], out _);
                        obj[segment] ??= nextSegmentIsArray ? new JsonArray() : new JsonObject();
                        current = obj[segment];
                    }
                }
            }
        }

        private static void EnsureArraySize(JsonArray array, int index)
        {
            while (array.Count <= index)
            {
                array.Add(null);
            }
        }
    }
}
