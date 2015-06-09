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

			if (lastStoredSequence != aggregate.SequenceID)
			{
				throw new Exception();
			}

			var changes = aggregate
				.GetUncommittedEvents()
				.Apply((e, i) => e.SequenceID = aggregate.SequenceID + i + 1)
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
	}
}
