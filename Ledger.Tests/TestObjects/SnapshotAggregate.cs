using System;
using System.Collections.Generic;
using Ledger.Infrastructure;

namespace Ledger.Tests.TestObjects
{
	public class SnapshotAggregate : SnapshotAggregateRoot<Guid, TestSnapshot>
	{
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

		protected override TestSnapshot CreateSnapshot()
		{
			return new TestSnapshot();
		}

		protected override void ApplySnapshot(TestSnapshot snapshot)
		{
		}
	}

	public class TestSnapshot : ISnapshot
	{
		public int SequenceID { get; set; }
	}
}
