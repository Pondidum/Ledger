using System;
using System.Linq;
using Ledger.Infrastructure;

namespace Ledger
{
	public class AggregateStore<TKey>
	{
		private readonly IEventStore _eventStore;

		public AggregateStore(IEventStore eventStore)
		{
			_eventStore = eventStore;
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
				.Apply((e, i) => e.AggregateID = aggregate.ID)
				.ToList();

			if (changes.None())
			{
				return;
			}

			var sni = GetSnapshotInterface<TAggregate>();

			if (sni != null)
			{
				var createSnapshot = aggregate
					.GetType()
					.GetMethod("CreateSnapshot");

				var snapshot = (ISnapshot)createSnapshot.Invoke(aggregate, new object[] { });
				snapshot.SequenceID = changes.Last().SequenceID;

				_eventStore.SaveSnapshot(aggregate.ID, snapshot);
			}

			_eventStore.SaveEvents(aggregate.ID, changes);

			aggregate.MarkEventsCommitted();

		}

		public TAggregate Load<TAggregate>(TKey aggregateID, Func<TAggregate> createNew)
			where TAggregate : AggregateRoot<TKey>
		{

			var aggregate = createNew();
			var sni = GetSnapshotInterface<TAggregate>();

			if (sni == null)
			{
				var events = _eventStore.LoadEvents(aggregateID);
				aggregate.LoadFromEvents(events);
			}
			else
			{
				var snapshot = _eventStore.GetLatestSnapshotFor(aggregate.ID);
				var events = _eventStore.LoadEventsSince(aggregateID, snapshot.SequenceID);

				aggregate.LoadFromSnapshot(snapshot, events);
			}

			return aggregate;
		}

		private static Type GetSnapshotInterface<TAggregate>()
		{
			return typeof(TAggregate)
				.GetInterfaces()
				.SingleOrDefault(i => i.GetGenericTypeDefinition() == typeof(ISnapshotable<>));
		}
	}
}
