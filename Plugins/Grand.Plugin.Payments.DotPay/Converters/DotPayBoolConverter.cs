using Newtonsoft.Json;
using System;

namespace Grand.Plugin.Payments.DotPay.Converters
{
    public class DotPayBoolConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value != null && (bool)value ? 1 : 0);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (objectType == typeof(bool?))
            {
                var value = reader.Value;
                return value != null ? value.ToString() == "1" : (bool?)null;
            }
            else
            {
                return reader.Value.ToString() == "1";
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(bool) || objectType == typeof(bool?);
        }
    }
}
