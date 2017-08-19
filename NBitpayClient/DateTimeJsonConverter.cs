using NBitcoin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBitpayClient
{
    class DateTimeJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTimeOffset);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var v = (long)reader.Value;
            Check(v);
            return unixRef + TimeSpan.FromMilliseconds((long)v);
        }

        static DateTimeOffset unixRef = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var date = ((DateTimeOffset)value).ToUniversalTime();
            long v = (long)(date - unixRef).TotalMilliseconds;
            Check(v);
            writer.WriteValue(v);
        }

        private static void Check(long v)
        {
            if(v < 0)
                throw new FormatException("Invalid datetime (less than 1/1/1970)");
        }
    }
}
