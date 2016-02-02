using System;
using System.Collections.Generic;

namespace Ledger
{
	public interface IStoreWriter<TKey> : IDisposable
	{
		int? GetLatestSequenceFor(TKey aggregateID);
		int GetNumberOfEventsSinceSnapshotFor(TKey aggregateID);

		void SaveEvents(IEnumerable<DomainEvent<TKey>> changes);
		void SaveSnapshot(ISnapshot<TKey> snapshot);
	}
}
