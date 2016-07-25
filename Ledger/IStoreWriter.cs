using System;
using System.Collections.Generic;
using Ledger.Infrastructure;

namespace Ledger
{
	public interface IStoreWriter<TKey> : IDisposable
	{
		Sequence? GetLatestSequenceFor(TKey aggregateID);
		int GetNumberOfEventsSinceSnapshotFor(TKey aggregateID);

		void SaveEvents(IEnumerable<DomainEvent<TKey>> changes);
		void SaveSnapshot(Snapshot<TKey> snapshot);
	}
}
