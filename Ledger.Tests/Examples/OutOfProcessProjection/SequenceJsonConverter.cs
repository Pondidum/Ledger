using System;
using Ledger.Infrastructure;
using Newtonsoft.Json;

namespace Ledger.Tests.Examples.OutOfProcessProjection
{
	public class SequenceJsonConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			writer.WriteValue((int)(Sequence)value);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			return new Sequence(Convert.ToInt32(reader.Value));
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(Sequence);
		}
	}
}