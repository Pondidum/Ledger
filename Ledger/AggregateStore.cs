﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ledger.Infrastructure;
using Newtonsoft.Json;

namespace Ledger
{
	public class AggregateStore<TKey>
	{
		private readonly IEventStore _eventStore;
		public SnapshotPolicy SnapshotPolicy { get; }
		public JsonSerializerSettings SerializerSettings { get; }

		public AggregateStore(IEventStore eventStore)
			: this(eventStore, new SnapshotPolicy())
		{
		}

		public AggregateStore(IEventStore eventStore, SnapshotPolicy policy)
			: this(eventStore, policy, Default.SerializerSettings)
		{
		}

		public AggregateStore(IEventStore eventStore, SnapshotPolicy policy, JsonSerializerSettings serializerSettings)
		{
			_eventStore = eventStore;
			SnapshotPolicy = policy;
			SerializerSettings = serializerSettings;
		}

		/// <summary>
		/// Save an aggregate
		/// </summary>
		/// <param name="stream">The stream to write to</param>
		/// <param name="aggregate">The aggregate to save</param>
		public void Save<TAggregate>(string stream, TAggregate aggregate)
			where TAggregate : AggregateRoot<TKey>
		{
			var changes = aggregate
				.GetUncommittedEvents()
				.Apply((e, i) => e.AggregateID = aggregate.ID)
				.Apply((e, i) => e.Sequence = aggregate.Sequence + i + 1)
				.ToList();

			if (changes.None())
			{
				return;
			}

			var context = new EventStoreContext(stream, SerializerSettings);

			using (var store = _eventStore.CreateWriter<TKey>(context))
			{
				ThrowIfVersionsInconsistent(store, aggregate);

				if (typeof(TAggregate).ImplementsSnapshottable() && SnapshotPolicy.NeedsSnapshot(store, aggregate, changes))
				{
					var snapshot = GetSnapshot(aggregate);
					snapshot.AggregateID = aggregate.ID;
					snapshot.Sequence = changes.Last().Sequence;
					snapshot.Stamp = changes.Last().Stamp;

					store.SaveSnapshot(snapshot);
				}

				store.SaveEvents(changes);

				aggregate.MarkEventsCommitted();
			}
		}

		private static void ThrowIfVersionsInconsistent<TAggregate>(IStoreWriter<TKey> store, TAggregate aggregate)
			where TAggregate : AggregateRoot<TKey>
		{
			var lastStoredSequence = store.GetLatestSequenceFor(aggregate.ID);

			if (lastStoredSequence.HasValue && lastStoredSequence != aggregate.Sequence)
			{
				throw new ConsistencyException(aggregate.GetType(), aggregate.ID.ToString(), aggregate.Sequence, lastStoredSequence);
			}
		}

		/// <summary>
		/// Load a specific aggregate
		/// </summary>
		/// <param name="stream">The stream to read from</param>
		/// <param name="aggregateID">The key of the aggregate to load</param>
		/// <param name="createNew">Invoked to create the blank aggregate instance</param>
		public TAggregate Load<TAggregate>(string stream, TKey aggregateID, Func<TAggregate> createNew)
			where TAggregate : AggregateRoot<TKey>
		{
			var context = new EventStoreContext(stream, SerializerSettings);

			using (var store = _eventStore.CreateReader<TKey>(context))
			{
				var aggregate = createNew();

				if (typeof(TAggregate).ImplementsSnapshottable())
				{
					var snapshot = store.LoadLatestSnapshotFor(aggregateID);
					DateTime? since = snapshot != null
						? snapshot.Stamp
						: (DateTime?)null;

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

		/// <summary>
		/// Loads all aggregates in the stream, not nessacarily in event order
		/// </summary>
		/// <param name="stream">The stream to read from</param>
		/// <param name="configureMapper">Configure which events and snapshots to create new aggregate instances on</param>
		public IEnumerable<AggregateRoot<TKey>> LoadAll(string stream, Action<AggregateLoadAllConfiguration<TKey>> configureMapper)
		{
			var loader = new AggregateLoadAllConfiguration<TKey>();
			configureMapper(loader);

			var context = new EventStoreContext(stream, SerializerSettings);

			using (var reader = _eventStore.CreateReader<TKey>(context))
			{
				var ids = reader.LoadAllKeys();

				foreach (var id in ids)
				{
					var snapshot = reader.LoadLatestSnapshotFor(id);
					DateTime? since = snapshot != null
						? snapshot.Stamp
						: (DateTime?)null;

					var events = reader.LoadEventsSince(id, since).GetEnumerator();
					var allEvents = Enumerable.Empty<DomainEvent<TKey>>();

					if (events.MoveNext())
						allEvents = allEvents.Concat(new[] { events.Current });

					allEvents = allEvents.Concat(new Iterator<DomainEvent<TKey>>(events));

					var creator = loader.For(snapshot, events.Current);
					var instance = creator();

					if (instance == null)
						continue;

					if (snapshot != null)
						instance.LoadFromSnapshot(snapshot, allEvents);
					else
						instance.LoadFromEvents(allEvents);

					yield return instance;
				}
			}
		}

		/// <summary>Yeilds all events in the store in order</summary>
		/// <param name="stream">The stream to replay</param>
		public IEnumerable<DomainEvent<TKey>> ReplayAll(string stream)
		{
			var context = new EventStoreContext(stream, SerializerSettings);

			using (var reader = _eventStore.CreateReader<TKey>(context))
			{
				return reader.LoadAllEvents();
			}
		}

		private ISnapshot<TKey> GetSnapshot<TAggregate>(TAggregate aggregate) where TAggregate : AggregateRoot<TKey>
		{
			//you could replace this method with `return (IStamped)(aggregate as dynamic).CreateSnapshot();`
			//but you loose the compiler checking the `CreateSnapshot` is the right method name etc.

			var methodName = TypeInfo.GetMethodName<ISnapshotable<TKey, ISnapshot<TKey>>>(x => x.CreateSnapshot());

			var createSnapshot = aggregate
				.GetType()
				.GetMethod(methodName);

			return (ISnapshot<TKey>)createSnapshot.Invoke(aggregate, new object[] { });
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
