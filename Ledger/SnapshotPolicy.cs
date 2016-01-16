using System.Collections.Generic;
using System.Linq;
using Ledger.Infrastructure;

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
	}
}
