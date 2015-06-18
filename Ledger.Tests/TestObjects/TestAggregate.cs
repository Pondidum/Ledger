using System;
using System.Collections.Generic;
using Ledger.Infrastructure;

namespace Ledger.Tests.TestObjects
{
	public class TestAggregate : AggregateRoot<Guid>
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
	}
}
