using System;
using System.Collections.Generic;
using System.Linq;
using Ledger.Infrastructure;

namespace Ledger
{
	public class AggregateStore<TKey>
	{
		private readonly IEventStore<TKey> _eventStore;
		public int DefaultSnapshotInterval { get; set; }

		public AggregateStore(IEventStore<TKey> eventStore)
		{
			_eventStore = eventStore;
			DefaultSnapshotInterval = 10;
		}

		public void Save<TAggregate>(TAggregate aggregate)
			where TAggregate : AggregateRoot<TKey>
		{
			using (var store = _eventStore.BeginTransaction())
			{
				ThrowIfVersionsInconsistent(store, aggregate);

				var changes = aggregate
					.GetUncommittedEvents()
					.Apply((e, i) => e.Sequence = aggregate.SequenceID + i)
					.ToList();

				if (changes.None())
				{
					return;
				}

				if (typeof(TAggregate).ImplementsSnapshottable() && NeedsSnapshot(store, aggregate, changes))
				{
					var methodName = TypeInfo.GetMethodName<ISnapshotable<ISequenced>>(x => x.CreateSnapshot());

					var createSnapshot = aggregate
						.GetType()
						.GetMethod(methodName);

					var snapshot = (ISequenced)createSnapshot.Invoke(aggregate, new object[] { });
					snapshot.Sequence = changes.Last().Sequence;

					store.SaveSnapshot(aggregate.ID, snapshot);
				}

				store.SaveEvents(aggregate.ID, changes);

				aggregate.MarkEventsCommitted();
			}
		}

		private static void ThrowIfVersionsInconsistent<TAggregate>(IEventStore<TKey> store, TAggregate aggregate)
			where TAggregate : AggregateRoot<TKey>
		{
			var lastStoredSequence = store.GetLatestSequenceFor(aggregate.ID);

			if (lastStoredSequence.HasValue && lastStoredSequence != aggregate.SequenceID)
			{
				throw new ConsistencyException(aggregate.GetType(), aggregate.ID.ToString(), aggregate.SequenceID, lastStoredSequence);
			}
		}

		private bool NeedsSnapshot<TAggregate>(IEventStore<TKey> store, TAggregate aggregate, IReadOnlyCollection<IDomainEvent> changes)
			where TAggregate : AggregateRoot<TKey>
		{
			var control = aggregate as ISnapshotControl;

			var interval = control != null
				? control.SnapshotInterval
				: DefaultSnapshotInterval;

			if (changes.Count >= interval)
			{
				return true;
			}

			var snapshotID = store.GetLatestSnapshotSequenceFor(aggregate.ID);

			return snapshotID.HasValue && changes.Last().Sequence >= snapshotID.Value + interval;
		}

		public TAggregate Load<TAggregate>(TKey aggregateID, Func<TAggregate> createNew)
			where TAggregate : AggregateRoot<TKey>
		{
			using (var store = _eventStore.BeginTransaction())
			{
				var aggregate = createNew();

				if (typeof(TAggregate).ImplementsSnapshottable())
				{
					var snapshot = store.LoadLatestSnapshotFor(aggregateID);
					var since = snapshot != null
						? snapshot.Sequence
						: -1;

					var events = store.LoadEventsSince(aggregateID, since);

					aggregate.LoadFromSnapshot(snapshot, events);
				}
				else
				{
					var events = store.LoadEvents(aggregateID);
					aggregate.LoadFromEvents(events);
				}

				return aggregate;
			}
		}
	}
}
