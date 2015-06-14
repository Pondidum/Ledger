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

		public void Save(AggregateRoot<TKey> aggregate)
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
				var snapshotType = sni.GetGenericArguments().Single();
				var getSnapshot = _eventStore
					.GetType()
					.GetMethod("GetLatestSnapshotFor")
					.MakeGenericMethod(typeof(TKey), snapshotType);

				var snapshot = (ISnapshot)getSnapshot.Invoke(_eventStore, new object[] { aggregateID });
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
