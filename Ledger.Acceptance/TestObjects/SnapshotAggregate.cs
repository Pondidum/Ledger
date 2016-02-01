using System;
using System.Collections.Generic;
using Ledger.Infrastructure;

namespace Ledger.Acceptance.TestObjects
{
	public class SnapshotAggregate : AggregateRoot<Guid>, ISnapshotable<Guid, TestSnapshot>
	{
		public SnapshotAggregate(Func<DateTime>  getTimestamp)
			:base(getTimestamp)
		{
		}

		public void AddEvent(DomainEvent<Guid> @event)
		{
			ApplyEvent(@event);
		}

		public void AddEvents(IEnumerable<DomainEvent<Guid>> events)
		{
			events.ForEach(AddEvent);
		}

		private void Handle(TestEvent @event) { }

		public void GenerateID()
		{
			ID = Guid.NewGuid();
		}

		public TestSnapshot CreateSnapshot()
		{
			return new TestSnapshot();
		}

		public void ApplySnapshot(TestSnapshot snapshot)
		{
		}

		public DateTime GetSequenceID()
		{
			return SequenceID;
		}
	}

	public class TestSnapshot : ISnapshot<Guid>
	{
		public Guid AggregateID { get; set; }
		public DateTime Stamp { get; set; }

	}
}
