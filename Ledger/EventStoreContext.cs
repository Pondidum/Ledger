using Newtonsoft.Json;

namespace Ledger
{
	public class EventStoreContext
	{
		public string StreamName { get; }
		public JsonSerializerSettings SerializerSettings { get; }

		public EventStoreContext(string streamName, JsonSerializerSettings serializerSettings)
		{
			StreamName = streamName;
			SerializerSettings = serializerSettings;
		}
	}
}
