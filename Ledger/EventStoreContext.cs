using Newtonsoft.Json;

namespace Ledger
{
	public class EventStoreContext
	{
		public string StreamName { get; }
		public JsonSerializerSettings SerializerSettings { get; }
		public ITypeResolver TypeResolver { get; }

		public EventStoreContext(string streamName, JsonSerializerSettings serializerSettings, ITypeResolver typeResolver)
		{
			StreamName = streamName;
			SerializerSettings = serializerSettings;
			TypeResolver = typeResolver;
		}
	}
}
