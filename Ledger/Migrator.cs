using Ledger.Infrastructure;
using Newtonsoft.Json;

namespace Ledger
{
	public class Migrator
	{
		private readonly IEventStore _source;
		private readonly IEventStore _destination;
		private readonly JsonSerializerSettings _settings;

		public Migrator(IEventStore source, IEventStore destination)
			: this(source, destination, null)
		{
		}

		public Migrator(IEventStore source, IEventStore destination, JsonSerializerSettings serializerSettings)
		{
			_source = source;
			_destination = destination;
			_settings = serializerSettings ?? Default.SerializerSettings;
		}

		public void ToEmptyStream<TKey>(string streamName)
		{
			var context = new EventStoreContext(streamName, _settings);

			using (var reader = _source.CreateReader<TKey>(context))
			using (var writer = _destination.CreateWriter<TKey>(context))
			{
				var keys = reader.LoadAllKeys();

				foreach (var key in keys)
				{
					writer.SaveEvents(reader.LoadEvents(key));

					var snapshot = reader.LoadLatestSnapshotFor(key);

					if (snapshot != null)
						writer.SaveSnapshot(snapshot);
				}
			}
		}
	}
}
