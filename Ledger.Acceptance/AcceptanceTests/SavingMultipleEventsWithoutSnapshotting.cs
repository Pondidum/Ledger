using System;
using System.Linq;
using Ledger.Acceptance.TestObjects;
using Shouldly;
using Xunit;

namespace Ledger.Acceptance.AcceptanceTests
{
	public class SavingMultipleEventsWithoutSnapshotting : AcceptanceBase<TestAggregate>
	{
		[Fact]
		public void When_saving_multiple_events_without_snapshotting()
		{
			var aggregateStore = new AggregateStore<Guid>(EventStore);
			var conventions = aggregateStore.Conventions<TestAggregate>();

			Aggregate = new TestAggregate();

			Aggregate.GenerateID();
			Aggregate.AddEvents(new[] { new TestEvent(), new TestEvent() });

			aggregateStore.Save(Aggregate);

			using (var reader = EventStore.CreateReader<Guid>(conventions))
			{
				var events = reader.LoadEvents(Aggregate.ID).ToList();

				reader.ShouldSatisfyAllConditions(
					() => events.Count().ShouldBe(2),
					() => events[0].Sequence.ShouldBe(0),
					() => events[1].Sequence.ShouldBe(1),
					() => Aggregate.GetUncommittedEvents().ShouldBeEmpty()
				);
			}
		}
	}
}
