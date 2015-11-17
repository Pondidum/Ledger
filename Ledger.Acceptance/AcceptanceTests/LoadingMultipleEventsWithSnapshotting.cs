using System;
using Ledger.Acceptance.TestObjects;
using Shouldly;
using Xunit;

namespace Ledger.Acceptance.AcceptanceTests
{
	public class LoadingMultipleEventsWithSnapshotting : AcceptanceBase<SnapshotAggregate>
	{
		public LoadingMultipleEventsWithSnapshotting()
		{
			var aggregateStore = new AggregateStore<Guid>(EventStore);
			var conventions = aggregateStore.Conventions<SnapshotAggregate>();
            var id = Guid.NewGuid();

			EventStore.CreateWriter<Guid>(conventions).SaveSnapshot(id, new TestSnapshot {Sequence = 10});
			EventStore.CreateWriter<Guid>(conventions).SaveEvents(id, new []
			{
				new TestEvent { Sequence = 5},
				new TestEvent { Sequence = 6},
			});

			Aggregate = aggregateStore.Load(id, () => new SnapshotAggregate());
		}

		[Fact]
		public void The_uncommitted_changes_should_be_empty()
		{
			Aggregate.GetUncommittedEvents().ShouldBeEmpty();
		}

		[Fact]
		public void The_sequence_id_should_be_the_last_events()
		{
			Aggregate.SequenceID.ShouldBe(10);
		}

	}
}
