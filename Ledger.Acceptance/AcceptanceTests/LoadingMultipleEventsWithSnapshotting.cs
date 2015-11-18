using System;
using Ledger.Acceptance.TestObjects;
using Shouldly;
using Xunit;

namespace Ledger.Acceptance.AcceptanceTests
{
	public class LoadingMultipleEventsWithSnapshotting : AcceptanceBase<SnapshotAggregate>
	{
		[Fact]
		public void When_loading_multiple_events_with_snapshotting()
		{
			var aggregateStore = new AggregateStore<Guid>(EventStore);
			var conventions = aggregateStore.Conventions<SnapshotAggregate>();
			var id = Guid.NewGuid();

			EventStore.CreateWriter<Guid>(conventions).SaveSnapshot(id, new TestSnapshot { Sequence = 10 });
			EventStore.CreateWriter<Guid>(conventions).SaveEvents(id, new[]
			{
				new TestEvent { Sequence = 5},
				new TestEvent { Sequence = 6},
			});

			Aggregate = aggregateStore.Load(id, () => new SnapshotAggregate());

			Aggregate.ShouldSatisfyAllConditions(
				() => Aggregate.GetUncommittedEvents().ShouldBeEmpty(),
				() => Aggregate.SequenceID.ShouldBe(10)
			);
		}
	}
}
