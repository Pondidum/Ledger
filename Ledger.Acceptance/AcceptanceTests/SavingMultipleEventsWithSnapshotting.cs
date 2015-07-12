using System;
using System.Collections.Generic;
using System.Linq;
using Ledger.Infrastructure;
using Ledger.Acceptance.TestObjects;
using Shouldly;
using Xunit;

namespace Ledger.Acceptance.AcceptanceTests
{
	public class SavingMultipleEventsWithSnapshotting : AcceptanceBase<SnapshotAggregate>
	{
		private readonly IEnumerable<DomainEvent> _events;

		public SavingMultipleEventsWithSnapshotting()
		{
			var aggregateStore = new AggregateStore<Guid>(EventStore);

			Aggregate = new SnapshotAggregate();
			_events = Enumerable
				.Range(0, aggregateStore.DefaultSnapshotInterval)
				.Select(i => new TestEvent { Sequence = i })
				.ToArray();

			Aggregate.GenerateID();
			Aggregate.AddEvents(_events);

			aggregateStore.Save(Aggregate);
		}

		[Fact]
		public void The_events_should_be_written()
		{
			EventStore
				.LoadEvents(Aggregate.ID)
				.ShouldNotBeEmpty();
		}

		[Fact]
		public void The_events_should_be_in_sequence()
		{
			_events
				.ForEach((e, i) => e.Sequence.ShouldBe(i));
		}

		[Fact]
		public void The_uncommitted_changes_should_be_cleared()
		{
			Aggregate
				.GetUncommittedEvents()
				.ShouldBeEmpty();
		}

		[Fact]
		public void The_snapshot_should_be_saved()
		{
			EventStore
				.LoadLatestSnapshotFor(Aggregate.ID)
				.ShouldNotBe(null);
		}

		[Fact]
		public void The_snapshot_should_have_the_latest_sequnce_id()
		{
			EventStore
				.LoadLatestSnapshotFor(Aggregate.ID)
				.Sequence
				.ShouldBe(_events.Count() - 1);
		}
	}
}
