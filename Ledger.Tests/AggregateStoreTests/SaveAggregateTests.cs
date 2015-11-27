using System;
using System.Collections.Generic;
using System.Linq;
using Ledger.Acceptance.TestObjects;
using Ledger.Infrastructure;
using Ledger.Stores;
using Shouldly;
using Xunit;

namespace Ledger.Tests
{
	public class SaveAggregateTests
	{
		private const string StreamName = "someStream";

		private readonly InMemoryEventStore _backing;
		private readonly AggregateStore<Guid> _store;

		public SaveAggregateTests()
		{
			_backing = new InMemoryEventStore();
			_store = new AggregateStore<Guid>(_backing);
		}

		[Fact]
		public void When_a_single_event_is_saved()
		{
			var aggregate = new TestAggregate();
			aggregate.GenerateID();

			aggregate.AddEvent(new TestEvent());

			_store.Save(StreamName, aggregate);

			var events = _backing
				.CreateReader<Guid>(StreamName)
				.LoadEvents(aggregate.ID)
				.ToList();

			aggregate.ShouldSatisfyAllConditions(
				() => events.ShouldAllBe(e => e.AggregateID == aggregate.ID),
				() => events.ForEach((e, i) => e.Sequence.ShouldBe(i)),
				() => aggregate.SequenceID.ShouldBe(0)
            );

		}

		[Fact]
		public void When_multiple_events_are_saved()
		{
			var aggregate = new TestAggregate();
			aggregate.GenerateID();

			aggregate.AddEvent(new TestEvent());
			aggregate.AddEvent(new TestEvent());
			aggregate.AddEvent(new TestEvent());
			aggregate.AddEvent(new TestEvent());

			_store.Save(StreamName, aggregate);

			var events = _backing
				.CreateReader<Guid>(StreamName)
				.LoadEvents(aggregate.ID)
				.ToList();

			aggregate.ShouldSatisfyAllConditions(
				() => events.ShouldAllBe(e => e.AggregateID == aggregate.ID),
				() => events.ForEach((e, i) => e.Sequence.ShouldBe(i)),
				() => aggregate.SequenceID.ShouldBe(3)
			);
		}


		[Fact]
		public void When_an_event_is_saved_onto_a_saved_aggregate()
		{
			var aggregate = new TestAggregate();
			aggregate.GenerateID();

			aggregate.AddEvent(new TestEvent()); //0
			aggregate.AddEvent(new TestEvent()); //1
			aggregate.AddEvent(new TestEvent()); //2
			aggregate.AddEvent(new TestEvent()); //3

			_store.Save(StreamName, aggregate);

			aggregate.AddEvent(new TestEvent()); //4

			_store.Save(StreamName, aggregate);

			var events = _backing
				.CreateReader<Guid>(StreamName)
				.LoadEvents(aggregate.ID)
				.ToList();

			aggregate.ShouldSatisfyAllConditions(
				() => events.ShouldAllBe(e => e.AggregateID == aggregate.ID),
				() => events.ForEach((e, i) => e.Sequence.ShouldBe(i)),
				() => aggregate.SequenceID.ShouldBe(4),
				() => events.Count.ShouldBe(5)
			);
		}

		[Fact]
		public void When_the_aggregate_implements_another_interface()
		{
			var aggregate = new InterfaceAggregate();
			aggregate.GenerateID();

			aggregate.AddEvent(new TestEvent());

			_store.Save(StreamName, aggregate);

			var events = _backing
				.CreateReader<Guid>(StreamName)
				.LoadEvents(aggregate.ID)
				.ToList();

			aggregate.ShouldSatisfyAllConditions(
				() => events.ShouldAllBe(e => e.AggregateID == aggregate.ID),
				() => events.ForEach((e, i) => e.Sequence.ShouldBe(i)),
				() => aggregate.SequenceID.ShouldBe(0)
			);
		}

		[Fact]
		public void When_the_aggregate_is_snapshottable()
		{
			var aggregate = new SnapshotAggregate();
			aggregate.GenerateID();

			Enumerable.Range(0, 12).ForEach((e,i) => aggregate.AddEvent(new TestEvent()));

			_store.Save(StreamName, aggregate);

			using (var reader = _backing.CreateReader<Guid>(StreamName))
			{
				var events = reader.LoadEvents(aggregate.ID).ToList();
				var snapshot = reader.LoadLatestSnapshotFor(aggregate.ID);

				aggregate.ShouldSatisfyAllConditions(
					() => events.Count.ShouldBe(12),
					() => snapshot.Sequence.ShouldBe(11),
					() => snapshot.AggregateID.ShouldBe(aggregate.ID)
				);
			}
		}

		[Fact]
		public void When_the_aggregate_needs_a_new_snapshot()
		{
			var aggregate = new SnapshotAggregate();
			aggregate.GenerateID();

			Enumerable.Range(0, 12).ForEach((e, i) => aggregate.AddEvent(new TestEvent()));
			_store.Save(StreamName, aggregate);

			Enumerable.Range(0, 12).ForEach((e, i) => aggregate.AddEvent(new TestEvent()));
			_store.Save(StreamName, aggregate);

			using (var reader = _backing.CreateReader<Guid>(StreamName))
			{
				var events = reader.LoadEvents(aggregate.ID).ToList();
				var snapshot = reader.LoadLatestSnapshotFor(aggregate.ID);

				aggregate.ShouldSatisfyAllConditions(
					() => events.Count.ShouldBe(24),
					() => snapshot.Sequence.ShouldBe(23),
					() => snapshot.AggregateID.ShouldBe(aggregate.ID)
				);
			}
		}


		public interface IKeyed
		{
			string Key { get; }
		}

		public class InterfaceAggregate : AggregateRoot<Guid>, IKeyed
		{
			public string Key { get; set; }

			public void AddEvent(DomainEvent<Guid> @event)
			{
				ApplyEvent(@event);
			}

			public void AddEvents(IEnumerable<DomainEvent<Guid>> events)
			{
				events.ForEach(AddEvent);
			}

			private void Handle(TestEvent @event) { }

			public void GenerateID()
			{
				ID = Guid.NewGuid();
			}
		}
	}
}
