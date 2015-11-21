using System;
using System.Collections.Generic;

namespace Ledger
{
	public interface IEventStore
	{
		IStoreReader<TKey> CreateReader<TKey>(IStoreConventions storeConventions);
		IStoreWriter<TKey> CreateWriter<TKey>(IStoreConventions storeConventions);
	}

	public interface IStoreReader<TKey> : IDisposable
	{
		IEnumerable<IDomainEvent<TKey>> LoadEvents(TKey aggregateID);
		IEnumerable<IDomainEvent<TKey>> LoadEventsSince(TKey aggregateID, int sequenceID);
		ISnapshot LoadLatestSnapshotFor(TKey aggregateID);
	}

	public interface IStoreWriter<TKey> : IDisposable
	{
		int? GetLatestSequenceFor(TKey aggregateID);
		int? GetLatestSnapshotSequenceFor(TKey aggregateID);

		void SaveEvents(TKey aggregateID, IEnumerable<IDomainEvent<TKey>> changes);
		void SaveSnapshot(TKey aggregateID, ISnapshot snapshot);
	}
}
