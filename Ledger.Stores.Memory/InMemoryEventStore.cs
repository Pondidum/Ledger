using System.Collections.Generic;
using System.Linq;

namespace Ledger.Stores.Memory
{
	public class InMemoryEventStore : IEventStore
	{
		public int? LatestSequenceID { get; set; }
		public int? LatestSnapshotID { get; set; }
		public List<object> Events { get; set; }
		public ISequenced Snapshot { get; set; }

		public InMemoryEventStore()
		{
			Events = new List<object>();
			Events = new List<object>();
			LatestSequenceID = null;
			Snapshot = null;
		}

		public int? GetLatestSequenceFor<TKey>(TKey aggegateID)
		{
			return LatestSequenceID;
		}

		public int? GetLatestSnapshotSequenceFor<TKey>(TKey aggregateID)
		{
			return LatestSnapshotID;
		}

		public void SaveEvents<TKey>(TKey aggegateID, IEnumerable<DomainEvent> changes)
		{
			Events.AddRange(changes);
		}

		public IEnumerable<DomainEvent> LoadEvents<TKey>(TKey aggegateID)
		{
			return Events.Cast<DomainEvent>();
		}

		public IEnumerable<DomainEvent> LoadEventsSince<TKey>(TKey aggegateID, int sequenceID)
		{
			return Events.Cast<DomainEvent>().Where(x => x.SequenceID > sequenceID);
		}

		public ISequenced LoadLatestSnapshotFor<TKey>(TKey aggegateID)
		{
			return Snapshot;
		}

		public void SaveSnapshot<TKey>(TKey aggregateID, ISequenced snapshot)
		{
			Snapshot = snapshot;
		}
	}
}