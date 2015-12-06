using System;
using System.Linq;
using Ledger.Acceptance.TestObjects;
using Ledger.Infrastructure;
using Shouldly;
using Xunit;

namespace Ledger.Acceptance
{
	public abstract class AcceptanceTests
	{
		public const string SnapshotStream = "SnapshotAggregateStream";
		public const string DefaultStream = "TestAggregateStream";

		private readonly IEventStore _eventStore;
		private readonly AggregateStore<Guid> _aggregateStore;
		
		protected AcceptanceTests(IEventStore eventStore)
		{
			_eventStore = eventStore;
			_aggregateStore = new AggregateStore<Guid>(_eventStore);
		}

		[Fact]
		public void When_there_are_no_events()
		{
			var aggregate = new SnapshotAggregate();
			aggregate.GenerateID();

			_aggregateStore.Save(SnapshotStream, aggregate);

			using (var reader = _eventStore.CreateReader<Guid>(SnapshotStream))
			{
				reader.LoadEvents(aggregate.ID).ShouldBeEmpty();
			}

		}

		[Fact]
		public void When_the_event_store_has_newer_events()
		{
			var aggregate = new SnapshotAggregate();
			aggregate.GenerateID();

			using (var writer = _eventStore.CreateWriter<Guid>(SnapshotStream))
			{
				writer.SaveEvents(new[] { new TestEvent { AggregateID = aggregate.ID, Sequence = 5 } });
			}

			aggregate.AddEvent(new TestEvent());

			Should.Throw<ConsistencyException>(() => _aggregateStore.Save(SnapshotStream, aggregate));
		}

		[Fact]
		public void When_loading_with_snapshotting_and_there_is_no_snapshot()
		{
			var id = Guid.NewGuid();

			using (var writer = _eventStore.CreateWriter<Guid>(SnapshotStream))
			{
				writer.SaveEvents(new[]
				{
					new TestEvent {AggregateID = id, Sequence = 5},
					new TestEvent {AggregateID = id, Sequence = 6},
				});
			}

			var aggregate = _aggregateStore.Load(SnapshotStream, id, () => new SnapshotAggregate());

			aggregate.GetSequenceID().ShouldBe(6);
		}

		[Fact]
		public void When_loading_multiple_events_without_snapshotting()
		{
			var id = Guid.NewGuid();

			using (var writer = _eventStore.CreateWriter<Guid>(DefaultStream))
			{
				writer.SaveEvents(new[]
				{
					new TestEvent {AggregateID = id, Sequence = 5},
					new TestEvent {AggregateID = id, Sequence = 6},
				});
			}

			var aggregate = _aggregateStore.Load(DefaultStream, id, () => new TestAggregate());

			aggregate.ShouldSatisfyAllConditions(
				() => aggregate.GetUncommittedEvents().ShouldBeEmpty(),
				() => aggregate.GetSequenceID().ShouldBe(6)
			);
		}

		[Fact]
		public void When_loading_multiple_events_with_snapshotting()
		{
			var id = Guid.NewGuid();

			using (var writer = _eventStore.CreateWriter<Guid>(SnapshotStream))
			{
				writer.SaveSnapshot(new TestSnapshot { AggregateID = id, Sequence = 10 });
				writer.SaveEvents(new[]
				{
					new TestEvent {AggregateID = id, Sequence = 5},
					new TestEvent {AggregateID = id, Sequence = 6},
				});
			}

			var aggregate = _aggregateStore.Load(SnapshotStream, id, () => new SnapshotAggregate());

			aggregate.ShouldSatisfyAllConditions(
				() => aggregate.GetUncommittedEvents().ShouldBeEmpty(),
				() => aggregate.GetSequenceID().ShouldBe(10)
			);
		}

		[Fact]
		public void When_saving_multiple_events_without_snapshotting()
		{
			var aggregate = new TestAggregate();

			aggregate.GenerateID();
			aggregate.AddEvents(new[] { new TestEvent(), new TestEvent() });

			_aggregateStore.Save(DefaultStream, aggregate);

			using (var reader = _eventStore.CreateReader<Guid>(DefaultStream))
			{
				var events = reader.LoadEvents(aggregate.ID).ToList();

				reader.ShouldSatisfyAllConditions(
					() => events.Count().ShouldBe(2),
					() => events[0].Sequence.ShouldBe(0),
					() => events[1].Sequence.ShouldBe(1),
					() => aggregate.GetUncommittedEvents().ShouldBeEmpty()
				);
			}
		}

		[Fact]
		public void When_saving_multiple_events_with_snapshotting()
		{
			var aggregate = new SnapshotAggregate();
			var events = Enumerable
				.Range(0, _aggregateStore.DefaultSnapshotInterval)
				.Select(i => new TestEvent { Sequence = i })
				.ToArray();

			aggregate.GenerateID();
			aggregate.AddEvents(events);

			_aggregateStore.Save(SnapshotStream, aggregate);

			using (var reader = _eventStore.CreateReader<Guid>(SnapshotStream))
			{
				var storeEvents = reader.LoadEvents(aggregate.ID);
				var storeSnapshot = reader.LoadLatestSnapshotFor(aggregate.ID);

				reader.ShouldSatisfyAllConditions(
					() => storeEvents.ShouldNotBeEmpty(),
					() => events.ForEach((e, i) => e.Sequence.ShouldBe(i)),
					() => aggregate.GetUncommittedEvents().ShouldBeEmpty(),
					() => storeSnapshot.ShouldNotBe(null),
					() => storeSnapshot.Sequence.ShouldBe(events.Length - 1)
				);
			}

		}
	}
}
