using Ledger.Infrastructure;
using Newtonsoft.Json;

namespace Ledger
{
	public class Migrator
	{
		private readonly IMigrationStrategy _strategy;

		public Migrator(IMigrationStrategy strategy)
		{
			_strategy = strategy;
		}

		public void ToEmptyStream<TKey>(IEventStore source, IEventStore destination, EventStoreContext context)
		{
			using (var reader = source.CreateReader<TKey>(context))
			using (var writer = destination.CreateWriter<TKey>(context))
			{
				_strategy.Execute(reader, writer);
			}
		}
	}
}
