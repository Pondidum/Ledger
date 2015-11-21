using System;
using System.Collections.Generic;
using Ledger.Infrastructure;

namespace Ledger.Acceptance.TestObjects
{
	public class SnapshotAggregate : AggregateRoot<Guid>, ISnapshotable<TestSnapshot>
	{
		public void AddEvent(IDomainEvent<Guid> @event)
		{
			ApplyEvent(@event);
		}

		public void AddEvents(IEnumerable<IDomainEvent<Guid>> events)
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
	}

	public class TestSnapshot : ISnapshot
	{
		public int Sequence { get; set; }
	}
}
