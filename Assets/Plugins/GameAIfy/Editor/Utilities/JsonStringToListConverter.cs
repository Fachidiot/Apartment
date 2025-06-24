using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameAIfySDK
{
    public class JsonStringToListConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            JToken token = JToken.Load(reader);

            if (token.Type == JTokenType.String)
            {
                var value = token.ToString();
                return JsonConvert.DeserializeObject<List<T>>(value);
            }
            else if (token.Type == JTokenType.Array)
            {
                return token.ToObject<List<T>>();
            }
            else
            {
                throw new JsonSerializationException($"Unexpected token type: {token.Type}");
            }
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}