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
		[Fact]
		public void When_saving_multiple_events_with_snapshotting()
		{
			var aggregateStore = new AggregateStore<Guid>(EventStore);
			var conventions = aggregateStore.Conventions<SnapshotAggregate>();

			Aggregate = new SnapshotAggregate();
			var events = Enumerable
				.Range(0, aggregateStore.DefaultSnapshotInterval)
				.Select(i => new TestEvent { Sequence = i })
				.ToArray();

			Aggregate.GenerateID();
			Aggregate.AddEvents(events);

			aggregateStore.Save(Aggregate);


			using (var reader = EventStore.CreateReader<Guid>(conventions))
			{
				var storeEvents = reader.LoadEvents(Aggregate.ID);
				var storeSnapshot = reader.LoadLatestSnapshotFor(Aggregate.ID);

				reader.ShouldSatisfyAllConditions(
					() => storeEvents.ShouldNotBeEmpty(),
					() => events.ForEach((e, i) => e.Sequence.ShouldBe(i)),
					() => Aggregate.GetUncommittedEvents().ShouldBeEmpty(),
					() => storeSnapshot.ShouldNotBe(null),
					() => storeSnapshot.Sequence.ShouldBe(events.Length - 1)
                );
			}

		}
	}
}
