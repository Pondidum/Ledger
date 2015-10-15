using System;
using System.Collections.Generic;
using System.Linq;
using Ledger.Infrastructure;
using Ledger.Acceptance.TestObjects;
using Shouldly;
using Xunit;

namespace Ledger.Acceptance.AcceptanceTests
{
	public class SavingMultipleEventsWithoutSnapshotting : AcceptanceBase<TestAggregate>
	{
		private IStoreConventions _storeConventions;

		public SavingMultipleEventsWithoutSnapshotting()
		{
			var aggregateStore = new AggregateStore<Guid>(EventStore);
			_storeConventions = aggregateStore.Conventions<TestAggregate>();

			Aggregate = new TestAggregate();

			Aggregate.GenerateID();
			Aggregate.AddEvents(new[] { new TestEvent(), new TestEvent() });

			aggregateStore.Save(Aggregate);
		}

		[Fact]
		public void The_events_should_be_written()
		{
			EventStore.LoadEvents(_storeConventions, Aggregate.ID).Count().ShouldBe(2);
		}

		[Fact]
		public void The_events_should_be_in_sequence()
		{
			var events = EventStore.LoadEvents(_storeConventions, Aggregate.ID).ToList();

			events.ShouldSatisfyAllConditions(
				() => events[0].Sequence.ShouldBe(0),
				() => events[1].Sequence.ShouldBe(1)
			);
		}

		[Fact]
		public void The_uncommitted_changes_should_be_cleared()
		{
			Aggregate.GetUncommittedEvents().ShouldBeEmpty();
		}
	}
}