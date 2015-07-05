using System;
using System.Collections.Generic;

namespace Ledger
{
	public interface IEventStore<TKey> : IDisposable
	{
		int? GetLatestSequenceFor(TKey aggregateID);
		int? GetLatestSnapshotSequenceFor(TKey aggregateID);
		
		void SaveEvents(TKey aggregateID, IEnumerable<DomainEvent> changes);
		IEnumerable<DomainEvent> LoadEvents(TKey aggregateID);
		IEnumerable<DomainEvent> LoadEventsSince(TKey aggregateID, int sequenceID);

		ISequenced LoadLatestSnapshotFor(TKey aggregateID);
		void SaveSnapshot(TKey aggregateID, ISequenced snapshot);

		IEventStore<TKey> BeginTransaction();
	}
}
