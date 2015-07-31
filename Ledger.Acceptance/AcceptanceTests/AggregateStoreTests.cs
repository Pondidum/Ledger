using System;
using Ledger.Acceptance.TestObjects;
using Shouldly;
using Xunit;

namespace Ledger.Acceptance.AcceptanceTests
{
	public class AggregateStoreTests : AcceptanceBase<SnapshotAggregate>
	{
		private readonly AggregateStore<Guid> _aggregateStore;

		public AggregateStoreTests()
		{
			_aggregateStore = new AggregateStore<Guid>(EventStore);
		}

		[Fact]
		public void When_there_are_no_events()
		{
			Aggregate = new SnapshotAggregate();
			Aggregate.GenerateID();

			_aggregateStore.Save(Aggregate);

			EventStore.LoadEvents(Aggregate.ID).ShouldBeEmpty();
		}

		[Fact]
		public void When_the_event_store_has_newer_events()
		{
			Aggregate = new SnapshotAggregate();
			Aggregate.GenerateID();

			EventStore.SaveEvents(Aggregate.ID, new[] { new TestEvent { Sequence = 5 } });

			Should.Throw<ConsistencyException>(() => _aggregateStore.Save(Aggregate));
		}

		[Fact]
		public void When_loading_with_snapshotting_and_there_is_no_snapshot()
		{
			var aggregateStore = new AggregateStore<Guid>(EventStore);
			var id = Guid.NewGuid();

			EventStore.SaveEvents(id, new[]
			{
				new TestEvent { Sequence = 5},
				new TestEvent { Sequence = 6},
			});

			Aggregate = aggregateStore.Load(id, () => new SnapshotAggregate());

			Aggregate.SequenceID.ShouldBe(6);
		}
	}
}
