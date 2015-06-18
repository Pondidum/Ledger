using System;
using System.Collections.Generic;
using Ledger.Infrastructure;

namespace Ledger.Tests.TestObjects
{
	public class SnapshotAggregate : AggregateRoot<Guid>, ISnapshotable<TestSnapshot>
	{
		public void AddEvent(DomainEvent @event)
		{
			ApplyEvent(@event);
		}

		public void AddEvents(IEnumerable<DomainEvent> events)
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

	public class TestSnapshot : ISequenced
	{
		public int SequenceID { get; set; }
	}
}
