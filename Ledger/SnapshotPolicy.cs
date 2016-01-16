using System.Collections.Generic;
using System.Linq;

namespace Ledger
{
	public class SnapshotPolicy
	{
		public int DefaultInterval { get; set; }

		public SnapshotPolicy()
		{
			DefaultInterval = 10;
		}

		public virtual bool NeedsSnapshot<TKey, TAggregate>(IStoreWriter<TKey> store, TAggregate aggregate, IReadOnlyCollection<IDomainEvent<TKey>> changes)
			where TAggregate : AggregateRoot<TKey>
		{
			var control = aggregate as ISnapshotControl;

			var interval = control != null
				? control.SnapshotInterval
				: DefaultInterval;

			if (changes.Count >= interval)
			{
				return true;
			}

			var eventCount = store.GetNumberOfEventsSinceSnapshotFor(aggregate.ID);

			return eventCount + changes.Count >= interval;
		}

		public virtual bool SupportsSnapshotting<TAggregate>()
		{
			return typeof(TAggregate)
				.GetInterfaces()
				.Where(i => i.IsGenericType)
				.Any(i => i.GetGenericTypeDefinition() == typeof(ISnapshotable<,>));
		}
	}
}
