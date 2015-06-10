using System;
using System.Collections.Generic;
using Ledger.Infrastructure;
using Ledger.Tests.TestObjects;
using Shouldly;
using Xunit;

namespace Ledger.Tests.AcceptanceTests
{
	public class SavingMultipleEventsWithoutSnapshotting
	{
		private readonly FakeEventStore _eventStore;
		private readonly TestAggregate _aggregate;
		private readonly IEnumerable<DomainEvent<Guid>> _events;

		public SavingMultipleEventsWithoutSnapshotting()
		{
			_eventStore = new FakeEventStore();
			var aggregateStore = new AggregateStore<Guid>(_eventStore);

			_aggregate = new TestAggregate();
			_events = new[] { new TestEvent(), new TestEvent() };

			_aggregate.GenerateID();
			_aggregate.AddEvents(_events);
			aggregateStore.Save(_aggregate);
		}

		[Fact]
		public void The_events_should_be_written()
		{
			_eventStore.WrittenToEvents.ShouldBe(_events);
		}

		[Fact]
		public void The_events_should_be_in_sequence()
		{
			_events.ForEach((e, i) => e.SequenceID.ShouldBe(i));
		}

		[Fact]
		public void The_events_should_have_the_aggregateID_set()
		{
			_events.ForEach(e => e.AggregateID.ShouldBe(_aggregate.ID));
		}

		[Fact]
		public void The_uncommitted_changes_should_be_cleared()
		{
			_aggregate.GetUncommittedEvents().ShouldBeEmpty();
		}
	}
}