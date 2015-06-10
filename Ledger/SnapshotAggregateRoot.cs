using System.Collections.Generic;

namespace Ledger
{
	public abstract class SnapshotAggregateRoot<TKey, TSnapshot> : AggregateRoot<TKey>
		where TSnapshot : ISnapshot
	{
		protected abstract TSnapshot CreateSnapshot();
		protected abstract void ApplySnapshot(TSnapshot snapshot);

		public void LoadFromSnapshot(TSnapshot snapshot, IEnumerable<DomainEvent<TKey>> events)
		{
			SequenceID = snapshot.SequenceID;
			LoadFromEvents(events);
		}
	}
}
