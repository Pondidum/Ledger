using System;
using System.Collections.Generic;

namespace Ledger
{
	public interface IEventStore
	{
		IStoreReader<TKey> CreateReader<TKey>();
		IStoreWriter<TKey> CreateWriter<TKey>();
	}

	public interface IStoreReader<TKey> : IDisposable
	{
		IEnumerable<IDomainEvent> LoadEvents(IStoreConventions storeConventions, TKey aggregateID);
		IEnumerable<IDomainEvent> LoadEventsSince(IStoreConventions storeConventions, TKey aggregateID, int sequenceID);
		ISequenced LoadLatestSnapshotFor(IStoreConventions storeConventions, TKey aggregateID);
	}

	public interface IStoreWriter<TKey> : IDisposable
	{
		int? GetLatestSequenceFor(IStoreConventions storeConventions, TKey aggregateID);
		int? GetLatestSnapshotSequenceFor(IStoreConventions storeConventions, TKey aggregateID);

		void SaveEvents(IStoreConventions storeConventions, TKey aggregateID, IEnumerable<IDomainEvent> changes);
		void SaveSnapshot(IStoreConventions storeConventions, TKey aggregateID, ISequenced snapshot);
	}
}
