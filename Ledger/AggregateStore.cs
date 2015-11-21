using System;
using System.Collections.Generic;
using System.Linq;
using Ledger.Conventions;
using Ledger.Infrastructure;

namespace Ledger
{
	public class AggregateStore<TKey>
	{
		private readonly IEventStore _eventStore;
		private readonly IStoreNamingConvention _namingConvention;
		public int DefaultSnapshotInterval { get; set; }

		public AggregateStore(IEventStore eventStore)
			: this(eventStore, new KeyTypeNamingConvention())
		{
		}

		public AggregateStore(IEventStore eventStore, IStoreNamingConvention namingConvention)
		{
			_eventStore = eventStore;
			_namingConvention = namingConvention;

			DefaultSnapshotInterval = 10;
		}

		public IStoreConventions Conventions<TAggregate>()
		{
			return new StoreConventions(_namingConvention, typeof (TKey), typeof (TAggregate));
		}

		public void Save<TAggregate>(TAggregate aggregate)
			where TAggregate : AggregateRoot<TKey>
		{
			var changes = aggregate
				.GetUncommittedEvents()
				.Apply((e, i) => e.AggregateID = aggregate.ID)
				.Apply((e, i) => e.Sequence = aggregate.SequenceID + i + 1)
				.ToList();

			if (changes.None())
			{
				return;
			}

			var conventions = Conventions<TAggregate>();

			using (var store = _eventStore.CreateWriter<TKey>(conventions))
			{
				ThrowIfVersionsInconsistent(store, aggregate);

				if (typeof(TAggregate).ImplementsSnapshottable() && NeedsSnapshot(store, aggregate, changes))
				{
					var snapshot = GetSnapshot(aggregate);
					snapshot.Sequence = changes.Last().Sequence;

					store.SaveSnapshot(aggregate.ID, snapshot);
				}

				store.SaveEvents(aggregate.ID, changes);

				aggregate.MarkEventsCommitted();
			}
		}

		private static ISnapshot<TKey> GetSnapshot<TAggregate>(TAggregate aggregate) where TAggregate : AggregateRoot<TKey>
		{
			//you could replace this method with `return (ISequenced)(aggregate as dynamic).CreateSnapshot();`
			//but you loose the compiler checking the `CreateSnapshot` is the right method name etc.

			var methodName = TypeInfo.GetMethodName<ISnapshotable<TKey, ISnapshot<TKey>>>(x => x.CreateSnapshot());

			var createSnapshot = aggregate
				.GetType()
				.GetMethod(methodName);

			return (ISnapshot<TKey>) createSnapshot.Invoke(aggregate, new object[] {});
		}

		private static void ThrowIfVersionsInconsistent<TAggregate>(IStoreWriter<TKey> store, TAggregate aggregate)
			where TAggregate : AggregateRoot<TKey>
		{
			var lastStoredSequence = store.GetLatestSequenceFor(aggregate.ID);

			if (lastStoredSequence.HasValue && lastStoredSequence != aggregate.SequenceID)
			{
				throw new ConsistencyException(aggregate.GetType(), aggregate.ID.ToString(), aggregate.SequenceID, lastStoredSequence);
			}
		}

		private bool NeedsSnapshot<TAggregate>(IStoreWriter<TKey> store, TAggregate aggregate, IReadOnlyCollection<IDomainEvent<TKey>> changes)
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
			var conventions = Conventions<TAggregate>();

			using (var store = _eventStore.CreateReader<TKey>(conventions))
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
