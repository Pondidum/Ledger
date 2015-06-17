using System.Collections.Generic;
using System.Linq;

namespace Ledger.Tests.TestObjects
{
	public class FakeEventStore : IEventStore
	{
		public int? LatestSequenceID { get; set; }
		public List<object> WrittenToEvents { get; set; }
		public List<object> ReadFromEvents { get; set; }
		public ISequenced Snapshot { get; set; }

		public FakeEventStore()
		{
			WrittenToEvents = new List<object>();
			ReadFromEvents = new List<object>();
			LatestSequenceID = null;
			Snapshot = null;
		}

		public int? GetLatestSequenceIDFor<TKey>(TKey aggegateID)
		{
			return LatestSequenceID;
		}

		public void SaveEvents<TKey>(TKey aggegateID, IEnumerable<DomainEvent<TKey>> changes)
		{
			WrittenToEvents.AddRange(changes);
		}

		public IEnumerable<DomainEvent<TKey>> LoadEvents<TKey>(TKey aggegateID)
		{
			return ReadFromEvents.Cast<DomainEvent<TKey>>();
		}

		public IEnumerable<DomainEvent<TKey>> LoadEventsSince<TKey>(TKey aggegateID, int sequenceID)
		{
			return ReadFromEvents.Cast<DomainEvent<TKey>>().Where(x => x.SequenceID > sequenceID);
		}

		public ISequenced GetLatestSnapshotFor<TKey>(TKey aggegateID)
		{
			return Snapshot;
		}

		public void SaveSnapshot<TKey>(TKey aggregateID, ISequenced snapshot)
		{
			Snapshot = snapshot;
		}
	}
}