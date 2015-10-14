using System;
using System.Collections.Generic;

namespace Ledger
{
	public interface IEventStore<TKey> : IDisposable
	{
		void Configure(IStoreNamingConvention namingConvention);

		int? GetLatestSequenceFor(TKey aggregateID);
		int? GetLatestSnapshotSequenceFor(TKey aggregateID);
		
		void SaveEvents(TKey aggregateID, IEnumerable<IDomainEvent> changes);
		IEnumerable<IDomainEvent> LoadEvents(TKey aggregateID);
		IEnumerable<IDomainEvent> LoadEventsSince(TKey aggregateID, int sequenceID);

		ISequenced LoadLatestSnapshotFor(TKey aggregateID);
		void SaveSnapshot(TKey aggregateID, ISequenced snapshot);

		IEventStore<TKey> BeginTransaction();
	}
}
