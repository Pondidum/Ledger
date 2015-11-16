using System.Collections.Generic;

namespace Ledger.Stores
{
	public class InterceptingEventStore<T> : IEventStore<T>
	{
		private readonly IEventStore<T> _other;

		public InterceptingEventStore(IEventStore<T> other)
		{
			_other = other;
		}

		public virtual void Dispose()
		{
			_other.Dispose();
		}

		public virtual int? GetLatestSequenceFor(IStoreConventions storeConventions, T aggregateID)
		{
			return _other.GetLatestSequenceFor(storeConventions, aggregateID);
		}

		public virtual int? GetLatestSnapshotSequenceFor(IStoreConventions storeConventions, T aggregateID)
		{
			return _other.GetLatestSnapshotSequenceFor(storeConventions, aggregateID);
		}

		public virtual void SaveEvents(IStoreConventions storeConventions, T aggregateID, IEnumerable<IDomainEvent> changes)
		{
			_other.SaveEvents(storeConventions, aggregateID, changes);
		}

		public virtual IEnumerable<IDomainEvent> LoadEvents(IStoreConventions storeConventions, T aggregateID)
		{
			return _other.LoadEvents(storeConventions, aggregateID);
		}

		public virtual IEnumerable<IDomainEvent> LoadEventsSince(IStoreConventions storeConventions, T aggregateID, int sequenceID)
		{
			return _other.LoadEventsSince(storeConventions, aggregateID, sequenceID);
		}

		public virtual ISequenced LoadLatestSnapshotFor(IStoreConventions storeConventions, T aggregateID)
		{
			return _other.LoadLatestSnapshotFor(storeConventions, aggregateID);
		}

		public virtual void SaveSnapshot(IStoreConventions storeConventions, T aggregateID, ISequenced snapshot)
		{
			_other.SaveSnapshot(storeConventions, aggregateID, snapshot);
		}

		public virtual IEventStore<T> BeginTransaction()
		{
			_other.BeginTransaction();
			return this;
		}
	}
}
