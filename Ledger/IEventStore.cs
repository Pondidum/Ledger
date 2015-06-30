using System.Collections.Generic;

namespace Ledger
{
	public interface IEventStore<TKey>
	{
		int? GetLatestSequenceFor(TKey aggegateID);
		int? GetLatestSnapshotSequenceFor(TKey aggregateID);
		
		void SaveEvents(TKey aggegateID, IEnumerable<DomainEvent> changes);
		IEnumerable<DomainEvent> LoadEvents(TKey aggegateID);
		IEnumerable<DomainEvent> LoadEventsSince(TKey aggegateID, int sequenceID);

		ISequenced LoadLatestSnapshotFor(TKey aggegateID);
		void SaveSnapshot(TKey aggregateID, ISequenced snapshot);
	}
}
