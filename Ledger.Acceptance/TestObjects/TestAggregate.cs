using System;
using System.Collections.Generic;
using Ledger.Infrastructure;

namespace Ledger.Acceptance.TestObjects
{
	public class TestAggregate : AggregateRoot<Guid>
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
	}
}
