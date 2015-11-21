using System;
using System.Collections.Generic;

namespace Ledger
{
	public interface IStoreWriter<TKey> : IDisposable
	{
		int? GetLatestSequenceFor(TKey aggregateID);
		int? GetLatestSnapshotSequenceFor(TKey aggregateID);

		void SaveEvents(TKey aggregateID, IEnumerable<IDomainEvent<TKey>> changes);
		void SaveSnapshot(TKey aggregateID, ISnapshot snapshot);
	}
}
