using System.Collections.Generic;

namespace Ledger.Stores
{
	public class InterceptingWriter<TKey> : IStoreWriter<TKey>
	{
		private readonly IStoreWriter<TKey> _other;

		public InterceptingWriter(IStoreWriter<TKey> other)
		{
			_other = other;
		}

		public virtual int? GetLatestSequenceFor(IStoreConventions storeConventions, TKey aggregateID)
		{
			return _other.GetLatestSequenceFor(storeConventions, aggregateID);
		}

		public virtual int? GetLatestSnapshotSequenceFor(IStoreConventions storeConventions, TKey aggregateID)
		{
			return _other.GetLatestSnapshotSequenceFor(storeConventions, aggregateID);
		}

		public virtual void SaveEvents(IStoreConventions storeConventions, TKey aggregateID, IEnumerable<IDomainEvent> changes)
		{
			_other.SaveEvents(storeConventions, aggregateID, changes);
		}

		public virtual void SaveSnapshot(IStoreConventions storeConventions, TKey aggregateID, ISequenced snapshot)
		{
			_other.SaveSnapshot(storeConventions, aggregateID, snapshot);
		}

		public virtual void Dispose()
		{
			_other.Dispose();
		}
	}
}
