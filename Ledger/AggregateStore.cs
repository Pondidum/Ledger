using System;
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

		public void Save<TAggregate>(TAggregate aggregate)
			where TAggregate : AggregateRoot<TKey>
		{
			var lastStoredSequence = _eventStore.GetLatestSequenceIDFor(aggregate.ID);

			if (lastStoredSequence.HasValue && lastStoredSequence != aggregate.SequenceID)
			{
				throw new Exception();
			}

			var changes = aggregate
				.GetUncommittedEvents()
				.Apply((e, i) => e.SequenceID = aggregate.SequenceID + i)
				.ToList();

			if (changes.None())
			{
				return;
			}

			if (ImplementsSnapshottable(aggregate) && NeedsSnapshot(aggregate, changes))
			{
				var methodName = TypeInfo.GetMethodName<ISnapshotable<ISequenced>>(x => x.CreateSnapshot());

				var createSnapshot = aggregate
					.GetType()
					.GetMethod(methodName);

				var snapshot = (ISequenced)createSnapshot.Invoke(aggregate, new object[] { });
				snapshot.SequenceID = changes.Last().SequenceID;

				_eventStore.SaveSnapshot(aggregate.ID, snapshot);
			}

			_eventStore.SaveEvents(aggregate.ID, changes);

			aggregate.MarkEventsCommitted();

		}

		private bool NeedsSnapshot<TAggregate>(TAggregate aggregate, IReadOnlyCollection<DomainEvent> changes)
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

			var snapshotID = _eventStore.GetLatestSnapshotIDFor(aggregate.ID);

			return snapshotID.HasValue && changes.Last().SequenceID >= snapshotID.Value + interval;
		}

		public TAggregate Load<TAggregate>(TKey aggregateID, Func<TAggregate> createNew)
			where TAggregate : AggregateRoot<TKey>
		{

			var aggregate = createNew();

			if (ImplementsSnapshottable(aggregate))
			{
				var snapshot = _eventStore.GetLatestSnapshotFor(aggregate.ID);
				var events = _eventStore.LoadEventsSince(aggregateID, snapshot.SequenceID);

				aggregate.LoadFromSnapshot(snapshot, events);
			}
			else
			{
				var events = _eventStore.LoadEvents(aggregateID);
				aggregate.LoadFromEvents(events);
			}

			return aggregate;
		}

		private static bool ImplementsSnapshottable(AggregateRoot<TKey> aggregate)
		{
			return aggregate
				.GetType()
				.GetInterfaces()
				.Any(i => i.GetGenericTypeDefinition() == typeof(ISnapshotable<>));
		}
	}
}
