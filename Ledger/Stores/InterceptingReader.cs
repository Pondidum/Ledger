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

		public virtual IEnumerable<IDomainEvent<TKey>> LoadEventsSince(TKey aggregateID, int sequenceID)
		{
			return _other.LoadEventsSince(aggregateID, sequenceID);
		}

		public virtual ISequenced LoadLatestSnapshotFor(TKey aggregateID)
		{
			return _other.LoadLatestSnapshotFor(aggregateID);
		}

		public virtual void Dispose()
		{
			_other.Dispose();
		}
	}
}
