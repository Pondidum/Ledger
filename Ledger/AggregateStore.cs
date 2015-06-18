using System;
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

			if (ImplementsSnapshottable<TAggregate>() && NeedsSnapshot(aggregate, changes.Last().SequenceID))
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

		private bool NeedsSnapshot<TAggregate>(TAggregate aggregate, int sequenceID)
		{
			var interval = DefaultSnapshotInterval;
			var control = aggregate as ISnapshotControl;

			if (control != null)
			{
				interval = control.SnapshotInterval;
			}

			// +1 due to 0 based index
			return (sequenceID + 1) % interval == 0;
		}

		public TAggregate Load<TAggregate>(TKey aggregateID, Func<TAggregate> createNew)
			where TAggregate : AggregateRoot<TKey>
		{

			var aggregate = createNew();

			if (ImplementsSnapshottable<TAggregate>())
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

		private static bool ImplementsSnapshottable<TAggregate>()
		{
			return typeof(TAggregate)
				.GetInterfaces()
				.Any(i => i.GetGenericTypeDefinition() == typeof(ISnapshotable<>));
		}
	}
}
