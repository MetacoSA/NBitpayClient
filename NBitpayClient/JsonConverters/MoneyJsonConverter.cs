using Newtonsoft.Json;
using System;
using System.Reflection;
using NBitcoin;

namespace NBitpayClient.JsonConverters
{
	public class MoneyJsonConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType.GetTypeInfo() == typeof(Money).GetTypeInfo();
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if(reader.TokenType == JsonToken.Null)
				return null;
			return Money.Parse((string)reader.Value);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if(value != null)
			{
				writer.WriteValue(value.ToString());
			}
		}
	}
}
