using System;
using System.Collections.Generic;
using System.Linq;
using Ledger.Infrastructure;
using Ledger.Tests.TestObjects;
using Shouldly;
using Xunit;

namespace Ledger.Tests.AcceptanceTests
{
	public class SavingMultipleEventsWithSnapshotting
	{
		private readonly FakeEventStore _eventStore;
		private readonly SnapshotAggregate _aggregate;
		private readonly IEnumerable<DomainEvent<Guid>> _events;

		public SavingMultipleEventsWithSnapshotting()
		{
			_eventStore = new FakeEventStore();
			var aggregateStore = new AggregateStore<Guid>(_eventStore);

			_aggregate = new SnapshotAggregate();
			_events = Enumerable
				.Range(0, aggregateStore.DefaultSnapshotInterval)
				.Select(i => new TestEvent())
				.ToArray();

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

		[Fact]
		public void The_snapshot_should_be_saved()
		{
			_eventStore.Snapshot.ShouldNotBe(null);
		}

		[Fact]
		public void The_snapshot_should_have_the_latest_sequnce_id()
		{
			_eventStore.Snapshot.SequenceID.ShouldBe(_events.Count() - 1);
		}
	}
}
