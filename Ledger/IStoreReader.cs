using System;
using System.Collections.Generic;

namespace Ledger
{
	public interface IStoreReader<TKey> : IDisposable
	{
		IEnumerable<IDomainEvent<TKey>> LoadEvents(TKey aggregateID);
		IEnumerable<IDomainEvent<TKey>> LoadEventsSince(TKey aggregateID, int sequenceID);
		ISnapshot<TKey> LoadLatestSnapshotFor(TKey aggregateID);
	}
}
