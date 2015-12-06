using System;
using System.Collections.Generic;

namespace Ledger
{
	public interface IStoreWriter<TKey> : IDisposable
	{
		DateTime? GetLatestStampFor(TKey aggregateID);
		int GetNumberOfEventsSinceSnapshotFor(TKey aggregateID);

		void SaveEvents(IEnumerable<IDomainEvent<TKey>> changes);
		void SaveSnapshot(ISnapshot<TKey> snapshot);
	}
}
