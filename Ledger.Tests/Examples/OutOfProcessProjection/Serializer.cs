using System;
using Newtonsoft.Json;

namespace Ledger.Tests.Examples.OutOfProcessProjection
{
	public static class Serializer
	{
		private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
		{
			ContractResolver = new IgnoreStreamSequenceContractResolver(),
			TypeNameHandling = TypeNameHandling.Auto,
			Converters = new JsonConverter[] { new SequenceJsonConverter() }
		};

		public static string Serialize(object value)
		{
			return JsonConvert.SerializeObject(value, Settings);
		}

		public static T Deserialize<T>(string json)
		{
			return JsonConvert.DeserializeObject<T>(json, Settings);
		}

		public static object Deserialize(string json, Type type)
		{
			return JsonConvert.DeserializeObject(json, type, Settings);
		}
	}
}