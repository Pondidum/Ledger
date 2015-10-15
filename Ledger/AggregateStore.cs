using System;
using System.Collections.Generic;
using System.Linq;
using Ledger.Conventions;
using Ledger.Infrastructure;

namespace Ledger
{
	public class AggregateStore<TKey>
	{
		private readonly IEventStore<TKey> _eventStore;
		private readonly IStoreNamingConvention _namingConvention;
		public int DefaultSnapshotInterval { get; set; }

		public AggregateStore(IEventStore<TKey> eventStore)
			: this(eventStore, new KeyTypeNamingConvention())
		{
		}

		public AggregateStore(IEventStore<TKey> eventStore, IStoreNamingConvention namingConvention)
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
				.Apply((e, i) => e.Sequence = aggregate.SequenceID + i)
				.ToList();

			if (changes.None())
			{
				return;
			}

			using (var store = _eventStore.BeginTransaction())
			{
				var conventions = Conventions<TAggregate>();

				ThrowIfVersionsInconsistent(store, conventions, aggregate);

				if (typeof(TAggregate).ImplementsSnapshottable() && NeedsSnapshot(store, aggregate, changes))
				{
					var snapshot = GetSnapshot(aggregate);
					snapshot.Sequence = changes.Last().Sequence;

					store.SaveSnapshot(conventions, aggregate.ID, snapshot);
				}

				store.SaveEvents(conventions, aggregate.ID, changes);

				aggregate.MarkEventsCommitted();
			}
		}

		private static ISequenced GetSnapshot<TAggregate>(TAggregate aggregate) where TAggregate : AggregateRoot<TKey>
		{
			//you could replace this method with `return (ISequenced)(aggregate as dynamic).CreateSnapshot();`
			//but you loose the compiler checking the `CreateSnapshot` is the right method name etc.

			var methodName = TypeInfo.GetMethodName<ISnapshotable<ISequenced>>(x => x.CreateSnapshot());

			var createSnapshot = aggregate
				.GetType()
				.GetMethod(methodName);

			return (ISequenced) createSnapshot.Invoke(aggregate, new object[] {});
		}

		private static void ThrowIfVersionsInconsistent<TAggregate>(IEventStore<TKey> store, IStoreConventions storeConventions, TAggregate aggregate)
			where TAggregate : AggregateRoot<TKey>
		{
			var lastStoredSequence = store.GetLatestSequenceFor(storeConventions, aggregate.ID);

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

			var conventions = Conventions<TAggregate>();
			var snapshotID = store.GetLatestSnapshotSequenceFor(conventions, aggregate.ID);

			return snapshotID.HasValue && changes.Last().Sequence >= snapshotID.Value + interval;
		}

		public TAggregate Load<TAggregate>(TKey aggregateID, Func<TAggregate> createNew)
			where TAggregate : AggregateRoot<TKey>
		{
			using (var store = _eventStore.BeginTransaction())
			{
				var conventions = Conventions<TAggregate>();
				var aggregate = createNew();

				if (typeof(TAggregate).ImplementsSnapshottable())
				{
					var snapshot = store.LoadLatestSnapshotFor(conventions, aggregateID);
					var since = snapshot != null
						? snapshot.Sequence
						: -1;

					var events = store.LoadEventsSince(conventions, aggregateID, since);

					aggregate.LoadFromSnapshot(snapshot, events);
				}
				else
				{
					var events = store.LoadEvents(conventions, aggregateID);
					aggregate.LoadFromEvents(events);
				}

				return aggregate;
			}
		}
	}
}
