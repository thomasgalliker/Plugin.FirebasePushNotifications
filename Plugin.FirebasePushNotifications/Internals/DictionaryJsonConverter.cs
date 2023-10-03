using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Plugin.FirebasePushNotifications.Internals
{
    public class DictionaryJsonConverter : JsonConverter
    {
        /// <summary>
        /// Creates a flat dictionary for <paramref name="obj"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="obj">Object to flatten.</param>
        /// <returns>Flat dictionary with path as key and value as string.</returns>
        public static IDictionary<string, string> Flatten<T>(T obj)
        {
            var jObject = JObject.FromObject(obj);
            return Flatten(jObject);
        }

        /// <summary>
        /// Creates a flat dictionary for <paramref name="obj"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="obj">Object to flatten.</param>
        /// <param name="jsonSerializer">Custom JsonSerializer used to serialize <paramref name="obj"/>.</param>
        /// <returns>Flat dictionary with path as key and value as string.</returns>
        public static IDictionary<string, string> Flatten<T>(T obj, JsonSerializer jsonSerializer)
        {
            var jObject = JObject.FromObject(obj, jsonSerializer);
            return Flatten(jObject);
        }

        /// <summary>
        /// Creates a flat dictionary for <paramref name="jsonObject"/> of type <seealso cref="JObject"/>.
        /// </summary>
        /// <param name="jsonObject">JObject to flatten.</param>
        /// <returns>Flat dictionary with path as key and value as string.</returns>
        public static IDictionary<string, string> Flatten(JObject jsonObject)
        {
            var jTokens = jsonObject
                .Descendants()
                .Where(p => !p.Any());

            var results = jTokens.Select(jToken =>
            {
                var value = (jToken as JValue)?.Value?.ToString();
                var key = jToken.Path;
                return new KeyValuePair<string, string>(key, value);
            }).ToDictionary(x => x.Key, x => x.Value);

            return results;
        }

        public static T Unflatten<T>(IDictionary<string, string> dictionary)
        {
            var jObject = Unflatten(dictionary);
            return jObject.ToObject<T>();
        }

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

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        /// 	<c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type objectType)
        {
            return typeof(IDictionary<string, object>).IsAssignableFrom(objectType);
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            this.WriteValue(writer, value);
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return this.ReadValue(reader);
        }

        private void WriteValue(JsonWriter writer, object value)
        {
            var t = JToken.FromObject(value);
            switch (t.Type)
            {
                case JTokenType.Object:
                    this.WriteObject(writer, value);
                    break;
                case JTokenType.Array:
                    this.WriteArray(writer, value);
                    break;
                default:
                    writer.WriteValue(value);
                    break;
            }
        }

        private void WriteObject(JsonWriter writer, object value)
        {
            writer.WriteStartObject();
            if (value is IDictionary<string, object> obj)
            {
                foreach (var kvp in obj)
                {
                    writer.WritePropertyName(kvp.Key);
                    this.WriteValue(writer, kvp.Value);
                }
            }

            writer.WriteEndObject();
        }

        private void WriteArray(JsonWriter writer, object value)
        {
            writer.WriteStartArray();
            var array = value as IEnumerable<object> ?? Enumerable.Empty<object>();
            foreach (var o in array)
            {
                this.WriteValue(writer, o);
            }

            writer.WriteEndArray();
        }

        private object ReadValue(JsonReader reader)
        {
            while (reader.TokenType == JsonToken.Comment)
            {
                if (!reader.Read())
                {
                    throw new JsonSerializationException("Unexpected Token when converting IDictionary<string, object>");
                }
            }

            switch (reader.TokenType)
            {
                case JsonToken.StartObject:
                    return this.ReadObject(reader);
                case JsonToken.StartArray:
                    return this.ReadArray(reader);
                case JsonToken.Integer:
                case JsonToken.Float:
                case JsonToken.String:
                case JsonToken.Boolean:
                case JsonToken.Undefined:
                case JsonToken.Null:
                case JsonToken.Date:
                case JsonToken.Bytes:
                    return reader.Value;
                default:
                    throw new JsonSerializationException
                        ($"Unexpected token when converting IDictionary<string, object>: {reader.TokenType}");
            }
        }

        private object ReadArray(JsonReader reader)
        {
            IList<object> values = new List<object>();

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.Comment:
                        break;
                    default:
                        var value = this.ReadValue(reader);
                        values.Add(value);
                        break;
                    case JsonToken.EndArray:
                        return values;
                }
            }

            throw new JsonSerializationException("Unexpected end when reading IDictionary<string, object>");
        }

        private object ReadObject(JsonReader reader)
        {
            var obj = new Dictionary<string, object>();

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        var propertyName = reader.Value.ToString();

                        if (!reader.Read())
                        {
                            throw new JsonSerializationException("Unexpected end when reading IDictionary<string, object>");
                        }

                        var value = this.ReadValue(reader);

                        obj[propertyName] = value;
                        break;
                    case JsonToken.Comment:
                        break;
                    case JsonToken.EndObject:
                        return obj;
                }
            }

            throw new JsonSerializationException("Unexpected end when reading IDictionary<string, object>");
        }

        private static JContainer UnflattenSingle(KeyValuePair<string, string> keyValue)
        {
            var path = keyValue.Key;
            var value = keyValue.Value;
            var pathSegments = SplitPath(path);

            JContainer lastItem = null;
            //build from leaf to root
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
                        throw new ArgumentOutOfRangeException();
                }
            }

            return lastItem;
        }

        public static IList<string> SplitPath(string path)
        {
            IList<string> result = new List<string>();
            var reg = new Regex(@"(?!\.)([^. ^\[\]]+)|(?!\[)(\d+)(?=\])");
            foreach (Match match in reg.Matches(path))
            {
                result.Add(match.Value);
            }

            return result;
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