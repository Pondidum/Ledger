using System.Collections.Generic;

namespace Ledger
{
	public interface IEventStore
	{
		int? GetLatestSequenceIDFor<TKey>(TKey aggegateID);
		
		void SaveEvents<TKey>(TKey aggegateID, IEnumerable<DomainEvent<TKey>> changes);
		IEnumerable<DomainEvent<TKey>> LoadEvents<TKey>(TKey aggegateID);
		IEnumerable<DomainEvent<TKey>> LoadEventsSince<TKey>(TKey aggegateID, int sequenceID);

		TSnapshot GetLatestSnapshotFor<TKey, TSnapshot>(TKey aggegateID);
	}
}