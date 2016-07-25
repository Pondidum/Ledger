using System;
using System.Collections.Generic;
using Ledger.Infrastructure;

namespace Ledger.Stores
{
	public class InterceptingWriter<TKey> : IStoreWriter<TKey>
	{
		private readonly IStoreWriter<TKey> _other;

		public InterceptingWriter(IStoreWriter<TKey> other)
		{
			_other = other;
		}

		public virtual Sequence? GetLatestSequenceFor(TKey aggregateID)
		{
			return _other.GetLatestSequenceFor(aggregateID);
		}

		public virtual int GetNumberOfEventsSinceSnapshotFor(TKey aggregateID)
		{
			return _other.GetNumberOfEventsSinceSnapshotFor(aggregateID);
		}

		public virtual void SaveEvents(IEnumerable<DomainEvent<TKey>> changes)
		{
			_other.SaveEvents(changes);
		}

		public virtual void SaveSnapshot(Snapshot<TKey> snapshot)
		{
			_other.SaveSnapshot(snapshot);
		}

		public virtual void Dispose()
		{
			_other.Dispose();
		}
	}
}
