using System;
using System.Collections.Generic;

namespace Ledger.Stores
{
	public class InterceptingReader<TKey> : IStoreReader<TKey>
	{
		private readonly IStoreReader<TKey> _other;

		public InterceptingReader(IStoreReader<TKey> other)
		{
			_other = other;
		}

		public virtual IEnumerable<IDomainEvent<TKey>> LoadEvents(TKey aggregateID)
		{
			return _other.LoadEvents(aggregateID);
		}

		public virtual IEnumerable<IDomainEvent<TKey>> LoadEventsSince(TKey aggregateID, DateTime sequenceID)
		{
			return _other.LoadEventsSince(aggregateID, sequenceID);
		}

		public virtual ISnapshot<TKey> LoadLatestSnapshotFor(TKey aggregateID)
		{
			return _other.LoadLatestSnapshotFor(aggregateID);
		}

		public IEnumerable<TKey> LoadAllKeys()
		{
			return _other.LoadAllKeys();
		}

		public virtual void Dispose()
		{
			_other.Dispose();
		}
	}
}
