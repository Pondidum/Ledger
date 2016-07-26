using System;
using System.Linq;
using Ledger.Acceptance.TestObjects;
using Ledger.Infrastructure;
using Newtonsoft.Json;
using Shouldly;
using Xunit;

namespace Ledger.Acceptance
{
	public abstract class AcceptanceTests
	{
		public readonly EventStoreContext SnapshotStream = new EventStoreContext( "SnapshotAggregateStream", new DefaultTypeResolver());
		public readonly EventStoreContext DefaultStream = new EventStoreContext("TestAggregateStream", new DefaultTypeResolver());

		private readonly IEventStore _eventStore;
		private readonly AggregateStore<Guid> _aggregateStore;

		protected AcceptanceTests(IEventStore eventStore)
		{
			_eventStore = eventStore;
			_aggregateStore = new AggregateStore<Guid>(_eventStore);
		}

		[Fact]
		[Trait("acceptance", "true")]
		public void When_there_are_no_events()
		{
			var aggregate = new SnapshotAggregate(DefaultStamper.Now);
			aggregate.GenerateID();

			_aggregateStore.Save(SnapshotStream.StreamName, aggregate);

			using (var reader = _eventStore.CreateReader<Guid>(SnapshotStream))
			{
				reader.LoadEvents(aggregate.ID).ShouldBeEmpty();
			}

		}

		[Fact]
		[Trait("acceptance", "true")]
		public void When_events_are_saved_globalSequence_is_set()
		{
			var id = Guid.NewGuid();
			var t1 = DateTime.UtcNow;
			var t2 = t1.AddSeconds(5);

			using (var writer = _eventStore.CreateWriter<Guid>(DefaultStream))
			{
				writer.SaveEvents(new[]
				{
					new TestEvent {AggregateID = id, Stamp = t1},
					new TestEvent {AggregateID = id, Stamp = t2},
				});
			}

			using (var reader = _eventStore.CreateReader<Guid>(DefaultStream))
			{
				var events = reader.LoadAllEvents().ToList();

				events.ShouldSatisfyAllConditions(
					() => events[0].StreamSequence.ShouldBe(new StreamSequence(0)),
					() => events[1].StreamSequence.ShouldBe(new StreamSequence(1))
				);
			}

		}

		[Fact]
		[Trait("acceptance", "true")]
		public void When_the_event_store_has_newer_events()
		{
			var aggregate = new SnapshotAggregate(DefaultStamper.Now);
			aggregate.GenerateID();

			using (var writer = _eventStore.CreateWriter<Guid>(SnapshotStream))
			{
				writer.SaveEvents(new[] { new TestEvent { AggregateID = aggregate.ID, Stamp = DateTime.UtcNow } });
			}

			aggregate.AddEvent(new TestEvent());

			Should.Throw<ConsistencyException>(() => _aggregateStore.Save(SnapshotStream.StreamName, aggregate));
		}

		[Fact]
		[Trait("acceptance", "true")]
		public void When_loading_with_snapshotting_and_there_is_no_snapshot()
		{
			var id = Guid.NewGuid();
			var t1 = DateTime.UtcNow;
			var t2 = t1.AddSeconds(5);

			using (var writer = _eventStore.CreateWriter<Guid>(SnapshotStream))
			{
				writer.SaveEvents(new[]
				{
					new TestEvent {AggregateID = id, Stamp = t1},
					new TestEvent {AggregateID = id, Stamp = t2},
				});
			}

			var aggregate = _aggregateStore.Load(SnapshotStream.StreamName, id, () => new SnapshotAggregate(DefaultStamper.Now));

			aggregate.GetSequenceID().ShouldBe(t2);
		}

		[Fact]
		[Trait("acceptance", "true")]
		public void When_loading_multiple_events_without_snapshotting()
		{
			var id = Guid.NewGuid();
			var t1 = DateTime.UtcNow;
			var t2 = t1.AddSeconds(5);

			using (var writer = _eventStore.CreateWriter<Guid>(DefaultStream))
			{
				writer.SaveEvents(new[]
				{
					new TestEvent {AggregateID = id, Stamp = t1},
					new TestEvent {AggregateID = id, Stamp = t2},
				});
			}

			var aggregate = _aggregateStore.Load(DefaultStream.StreamName, id, () => new TestAggregate(DefaultStamper.Now));

			aggregate.ShouldSatisfyAllConditions(
				() => aggregate.GetUncommittedEvents().ShouldBeEmpty(),
				() => aggregate.GetSequenceID().ShouldBe(t2)
			);
		}

		[Fact]
		[Trait("acceptance", "true")]
		public void When_loading_multiple_events_with_snapshotting()
		{
			var id = Guid.NewGuid();

			var t5 = DateTime.UtcNow;
			var t6 = t5.AddSeconds(1);
			var t10 = t5.AddSeconds(5);

			using (var writer = _eventStore.CreateWriter<Guid>(SnapshotStream))
			{
				writer.SaveSnapshot(new TestSnapshot { AggregateID = id, Stamp = t10 });
				writer.SaveEvents(new[]
				{
					new TestEvent {AggregateID = id, Stamp = t5},
					new TestEvent {AggregateID = id, Stamp = t6},
				});
			}

			var aggregate = _aggregateStore.Load(SnapshotStream.StreamName, id, () => new SnapshotAggregate(DefaultStamper.Now));

			aggregate.ShouldSatisfyAllConditions(
				() => aggregate.GetUncommittedEvents().ShouldBeEmpty(),
				() => aggregate.GetSequenceID().ShouldBe(t10)
			);
		}

		[Fact]
		[Trait("acceptance", "true")]
		public void When_saving_multiple_events_without_snapshotting()
		{
			var start = DefaultStamper.Now();
			var offset = 0;
			var stamper = new Func<DateTime>(() => start.AddSeconds(offset++));

			var aggregate = new TestAggregate(stamper);

			aggregate.GenerateID();
			aggregate.AddEvents(new[] { new TestEvent(), new TestEvent() });

			_aggregateStore.Save(DefaultStream.StreamName, aggregate);

			using (var reader = _eventStore.CreateReader<Guid>(DefaultStream))
			{
				var events = reader.LoadEvents(aggregate.ID).ToList();

				reader.ShouldSatisfyAllConditions(
					() => events.Count().ShouldBe(2),
					() => events[0].Stamp.ShouldBe(start),
					() => events[1].Stamp.ShouldBe(start.AddSeconds(1)),
					() => aggregate.GetUncommittedEvents().ShouldBeEmpty()
				);
			}
		}

		[Fact]
		[Trait("acceptance", "true")]
		public void When_saving_multiple_events_with_snapshotting()
		{

			var start = DefaultStamper.Now();
			var offset = 0;
			var stamper = new Func<DateTime>(() => start.AddSeconds(offset++));

			var aggregate = new SnapshotAggregate(stamper);
			var events = Enumerable
				.Range(0, _aggregateStore.SnapshotPolicy.DefaultInterval)
				.Select(i => new TestEvent())
				.ToArray();

			aggregate.GenerateID();
			aggregate.AddEvents(events);

			_aggregateStore.Save(SnapshotStream.StreamName, aggregate);

			using (var reader = _eventStore.CreateReader<Guid>(SnapshotStream))
			{
				var storeEvents = reader.LoadEvents(aggregate.ID);
				var storeSnapshot = reader.LoadLatestSnapshotFor(aggregate.ID);

				reader.ShouldSatisfyAllConditions(
					() => storeEvents.ShouldNotBeEmpty(),
					() => events.ForEach((e, i) => e.Stamp.ShouldBe(start.AddSeconds(i))),
					() => aggregate.GetUncommittedEvents().ShouldBeEmpty(),
					() => storeSnapshot.ShouldNotBe(null),
					() => storeSnapshot.Stamp.ShouldBe(events.Last().Stamp)
				);
			}

		}
	}
}
