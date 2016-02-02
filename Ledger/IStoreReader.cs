using System;
using System.Collections.Generic;

namespace Ledger
{
	public interface IStoreReader<TKey> : IDisposable
	{
		IEnumerable<DomainEvent<TKey>> LoadEvents(TKey aggregateID);
		IEnumerable<DomainEvent<TKey>> LoadEventsSince(TKey aggregateID, DateTime? stamp);
		Snapshot<TKey> LoadLatestSnapshotFor(TKey aggregateID);

		IEnumerable<TKey> LoadAllKeys();
		IEnumerable<DomainEvent<TKey>> LoadAllEvents();
	}
}
