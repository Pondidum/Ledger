using System.Collections.Generic;

namespace Ledger
{
	public interface IEventStore
	{
		int? GetLatestSequenceIDFor<TKey>(TKey aggegateID);
		int? GetLatestSnapshotIDFor<TKey>(TKey aggregateID);
		
		void SaveEvents<TKey>(TKey aggegateID, IEnumerable<DomainEvent> changes);
		IEnumerable<DomainEvent> LoadEvents<TKey>(TKey aggegateID);
		IEnumerable<DomainEvent> LoadEventsSince<TKey>(TKey aggegateID, int sequenceID);

		ISequenced GetLatestSnapshotFor<TKey>(TKey aggegateID);
		void SaveSnapshot<TKey>(TKey aggregateID, ISequenced snapshot);
	}
}
