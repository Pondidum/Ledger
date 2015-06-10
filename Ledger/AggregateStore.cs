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

		public TAggregate Load<TAggregate>(TKey aggegateID, Func<TAggregate> createNew)
			where TAggregate : AggregateRoot<TKey>
		{
			var events = _eventStore.LoadEvents(aggegateID);

			var aggregate = createNew();
			aggregate.LoadFromEvents(events);

			return aggregate;
		}

		public TAggregate Load<TAggregate, TSnapshot>(TKey aggegateID, Func<TAggregate> createNew)
			where TAggregate : AggregateRoot<TKey>, ISnapshotable<TSnapshot>
			where TSnapshot : ISnapshot
		{
			var snapshot = _eventStore.GetLatestSnapshotFor<TKey, TSnapshot>(aggegateID);

			if (snapshot == null)
			{
				return Load(aggegateID, createNew);
			}

			var events = _eventStore.LoadEventsSince(aggegateID, snapshot.SequenceID);

			var aggregate = createNew();
			aggregate.ApplySnapshot(snapshot);
			aggregate.LoadFromEvents(events);

			return aggregate;
		}
	}
}
