using System;
using System.Collections.Generic;
using System.Linq;
using Ledger.Infrastructure;
using Ledger.Tests.TestObjects;
using Shouldly;
using Xunit;

namespace Ledger.Tests.AcceptanceTests
{
	public class SavingMultipleEventsWithoutSnapshotting : AcceptanceBase<TestAggregate>
	{
		public SavingMultipleEventsWithoutSnapshotting()
		{
			var aggregateStore = new AggregateStore<Guid>(EventStore);

			Aggregate = new TestAggregate();

			Aggregate.GenerateID();
			Aggregate.AddEvents(new[] { new TestEvent(), new TestEvent() });
		
			aggregateStore.Save(Aggregate);
		}

		[Fact]
		public void The_events_should_be_written()
		{
			EventStore.LoadEvents(Aggregate.ID).Count().ShouldBe(2);
		}

		[Fact]
		public void The_events_should_be_in_sequence()
		{
			var events = EventStore.LoadEvents(Aggregate.ID).ToList();

			events.ShouldSatisfyAllConditions(
				() => events[0].SequenceID.ShouldBe(0),
				() => events[1].SequenceID.ShouldBe(1)
			);
		}

		[Fact]
		public void The_uncommitted_changes_should_be_cleared()
		{
			Aggregate.GetUncommittedEvents().ShouldBeEmpty();
		}
	}
}