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

		public virtual int? GetLatestSequenceFor(TKey aggregateID)
		{
			return _other.GetLatestSequenceFor(aggregateID);
		}

		public virtual int? GetLatestSnapshotSequenceFor(TKey aggregateID)
		{
			return _other.GetLatestSnapshotSequenceFor(aggregateID);
		}

		public virtual void SaveEvents(TKey aggregateID, IEnumerable<IDomainEvent<TKey>> changes)
		{
			_other.SaveEvents(aggregateID, changes);
		}

		public virtual void SaveSnapshot(TKey aggregateID, ISnapshot snapshot)
		{
			_other.SaveSnapshot(aggregateID, snapshot);
		}

		public virtual void Dispose()
		{
			_other.Dispose();
		}
	}
}
