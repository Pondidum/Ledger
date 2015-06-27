using System;
using System.Collections.Generic;
using Ledger.Infrastructure;
using Ledger.Tests.TestObjects;
using Shouldly;
using Xunit;

namespace Ledger.Tests.AcceptanceTests
{
	public class SavingMultipleEventsWithoutSnapshotting : AcceptanceBase<TestAggregate>
	{
		private readonly IEnumerable<DomainEvent> _events;

		public SavingMultipleEventsWithoutSnapshotting()
		{
			var aggregateStore = new AggregateStore<Guid>(EventStore);

			Aggregate = new TestAggregate();
			_events = new[] { new TestEvent(), new TestEvent() };

			Aggregate.GenerateID();
			Aggregate.AddEvents(_events);
		
			aggregateStore.Save(Aggregate);
		}

		[Fact]
		public void The_events_should_be_written()
		{
			EventStore.LoadEvents(Aggregate.ID).ShouldBe(_events);
		}

		[Fact]
		public void The_events_should_be_in_sequence()
		{
			_events.ForEach((e, i) => e.SequenceID.ShouldBe(i));
		}

		[Fact]
		public void The_uncommitted_changes_should_be_cleared()
		{
			Aggregate.GetUncommittedEvents().ShouldBeEmpty();
		}
	}
}