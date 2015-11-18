using System;
using Ledger.Acceptance.TestObjects;
using Shouldly;
using Xunit;

namespace Ledger.Acceptance.AcceptanceTests
{
	public class LoadingMultipleEventsWithoutSnapshotting : AcceptanceBase<TestAggregate>
	{
		[Fact]
		public void When_loading_multiple_events_without_snapshotting()
		{
			var aggregateStore = new AggregateStore<Guid>(EventStore);
			var id = Guid.NewGuid();

			EventStore.CreateWriter<Guid>(aggregateStore.Conventions<TestAggregate>()).SaveEvents(id, new[]
			{
				new TestEvent { Sequence = 5},
				new TestEvent { Sequence = 6},
			});

			Aggregate = aggregateStore.Load(id, () => new TestAggregate());

			Aggregate.ShouldSatisfyAllConditions(
				() => Aggregate.GetUncommittedEvents().ShouldBeEmpty(),
				() => Aggregate.SequenceID.ShouldBe(6)
			);
		}
	}
}
