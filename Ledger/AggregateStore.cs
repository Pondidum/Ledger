using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ledger.Infrastructure;

namespace Ledger
{
	public class AggregateStore<TKey>
	{
		private readonly IEventStore _eventStore;
		public int DefaultSnapshotInterval { get; set; }

		public AggregateStore(IEventStore eventStore)
		{
			_eventStore = eventStore;

			DefaultSnapshotInterval = 10;
		}

		public void Save<TAggregate>(string stream, TAggregate aggregate)
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

			using (var store = _eventStore.CreateWriter<TKey>(stream))
			{
				ThrowIfVersionsInconsistent(store, aggregate);

				if (typeof(TAggregate).ImplementsSnapshottable() && NeedsSnapshot(store, aggregate, changes))
				{
					var snapshot = GetSnapshot(aggregate);
					snapshot.AggregateID = aggregate.ID;
					snapshot.Sequence = changes.Last().Sequence;

					store.SaveSnapshot(snapshot);
				}

				store.SaveEvents(changes);

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

			return (ISnapshot<TKey>)createSnapshot.Invoke(aggregate, new object[] { });
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

		public TAggregate Load<TAggregate>(string stream, TKey aggregateID, Func<TAggregate> createNew)
			where TAggregate : AggregateRoot<TKey>
		{
			using (var store = _eventStore.CreateReader<TKey>(stream))
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

		public IEnumerable<AggregateRoot<TKey>> LoadAll(string stream, Action<AggregateLoadAllConfiguration<TKey>> configureMapper)
		{
			var loader = new AggregateLoadAllConfiguration<TKey>();
			configureMapper(loader);

			using (var reader = _eventStore.CreateReader<TKey>(stream))
			{
				var ids = reader.LoadAllKeys();

				foreach (var id in ids)
				{
					var snapshot = reader.LoadLatestSnapshotFor(id);
					var since = snapshot != null
						? snapshot.Sequence
						: -1;

					var events = reader.LoadEventsSince(id, since).GetEnumerator();
					var allEvents = Enumerable.Empty<IDomainEvent<TKey>>();

					if (events.MoveNext())
						allEvents = allEvents.Concat(new[] {events.Current});

					allEvents = allEvents.Concat(new Iterator<IDomainEvent<TKey>>(events));

					var creator = loader.For(snapshot, events.Current);
					var instance = creator();

					if (snapshot != null)
						instance.LoadFromSnapshot(snapshot, allEvents);
					else
						instance.LoadFromEvents(allEvents);

					yield return instance;
				}
			}
		}

		private class Iterator<T> : IEnumerable<T>
		{
			private readonly IEnumerator<T> _enumerator;

			public Iterator(IEnumerator<T> enumerator)
			{
				_enumerator = enumerator;
			}

			public IEnumerator<T> GetEnumerator()
			{
				return _enumerator;
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}
	}
}
