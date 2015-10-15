using System;
using System.Collections.Generic;

namespace Ledger
{
	public interface IEventStore<TKey> : IDisposable
	{
		int? GetLatestSequenceFor(IStoreConventions storeConventions, TKey aggregateID);
		int? GetLatestSnapshotSequenceFor(IStoreConventions storeConventions, TKey aggregateID);
		
		void SaveEvents(IStoreConventions storeConventions, TKey aggregateID, IEnumerable<IDomainEvent> changes);
		IEnumerable<IDomainEvent> LoadEvents(IStoreConventions storeConventions, TKey aggregateID);
		IEnumerable<IDomainEvent> LoadEventsSince(IStoreConventions storeConventions, TKey aggregateID, int sequenceID);

		ISequenced LoadLatestSnapshotFor(IStoreConventions storeConventions, TKey aggregateID);
		void SaveSnapshot(IStoreConventions storeConventions, TKey aggregateID, ISequenced snapshot);

		IEventStore<TKey> BeginTransaction();
	}
}
