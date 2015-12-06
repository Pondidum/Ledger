using System;
using System.Collections.Generic;

namespace Ledger
{
	public interface IStoreWriter<TKey> : IDisposable
	{
		DateTime? GetLatestSequenceFor(TKey aggregateID);
		int GetNumberOfEventsSinceSnapshotFor(TKey aggregateID);

		void SaveEvents(IEnumerable<IDomainEvent<TKey>> changes);
		void SaveSnapshot(ISnapshot<TKey> snapshot);
	}
}
