using System;
using System.Collections.Generic;
using System.Linq;
using Ledger.Infrastructure;
using Ledger.Tests.TestObjects;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Ledger.Tests
{
	public class AggregateStoreTests
	{
		private readonly FakeEventStore _eventStore;
		private readonly AggregateStore<Guid> _aggregateStore;

		public AggregateStoreTests()
		{
			_eventStore = new FakeEventStore();
			_aggregateStore = new AggregateStore<Guid>(_eventStore);
		}

		[Fact]
		public void When_there_are_no_events()
		{
			_eventStore.LatestSequenceID = null;

			_aggregateStore.Save(new TestAggregate());

			_eventStore.WrittenToEvents.ShouldBeEmpty();
		}

		[Fact]
		public void When_the_event_store_has_newer_events()
		{
			_eventStore.LatestSequenceID = 5;

			Should.Throw<Exception>(() => _aggregateStore.Save(new TestAggregate()));
		}
	}

	public class When_saving_multiple_events
	{
		private readonly FakeEventStore _eventStore;
		private readonly AggregateStore<Guid> _aggregateStore;
		private readonly TestAggregate _aggregate;
		private readonly IEnumerable<DomainEvent<Guid>> _events;

		public When_saving_multiple_events()
		{
			_eventStore = new FakeEventStore();
			_aggregateStore = new AggregateStore<Guid>(_eventStore);

			_aggregate = new TestAggregate();
			_events = new[] { new TestEvent(), new TestEvent() };

			_aggregate.GenerateID();
			_aggregate.AddEvents(_events);
			_aggregateStore.Save(_aggregate);
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
