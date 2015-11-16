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

		public virtual IEnumerable<IDomainEvent> LoadEvents(IStoreConventions storeConventions, TKey aggregateID)
		{
			return _other.LoadEvents(storeConventions, aggregateID);
		}

		public virtual IEnumerable<IDomainEvent> LoadEventsSince(IStoreConventions storeConventions, TKey aggregateID, int sequenceID)
		{
			return _other.LoadEventsSince(storeConventions, aggregateID, sequenceID);
		}

		public virtual ISequenced LoadLatestSnapshotFor(IStoreConventions storeConventions, TKey aggregateID)
		{
			return _other.LoadLatestSnapshotFor(storeConventions, aggregateID);
		}

		public virtual void Dispose()
		{
			_other.Dispose();
		}
	}
}
