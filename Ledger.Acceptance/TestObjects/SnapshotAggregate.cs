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
			return Stamp;
		}
	}

	public class TestSnapshot : Snapshot<Guid>
	{
		public override Guid AggregateID { get; set; }
		public override Sequence Sequence { get; set; }
		public override DateTime Stamp { get; set; }
	}
}
