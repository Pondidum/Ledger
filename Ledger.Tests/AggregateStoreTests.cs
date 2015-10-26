using System;
using System.Linq;
using Ledger.Acceptance.TestObjects;
using Ledger.Stores;
using Shouldly;
using Xunit;

namespace Ledger.Tests
{
	public class AggregateStoreTests
	{
		private readonly InMemoryEventStore<Guid> _backing;
		private readonly AggregateStore<Guid> _store;

		public AggregateStoreTests()
		{
			_backing = new InMemoryEventStore<Guid>();
			_store = new AggregateStore<Guid>(_backing);
		}

		[Fact]
		public void When_a_single_event_is_saved()
		{
			var aggregate = new TestAggregate();
			aggregate.GenerateID();

			aggregate.AddEvent(new TestEvent());

			_store.Save(aggregate);

			_backing
				.LoadEvents(_store.Conventions<TestAggregate>(), aggregate.ID)
				.Last()
				.Sequence
				.ShouldBe(0);

			aggregate
				.SequenceID
				.ShouldBe(0);
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

			_store.Save(aggregate);

			_backing
				.LoadEvents(_store.Conventions<TestAggregate>(), aggregate.ID)
				.Last()
				.Sequence
				.ShouldBe(3);

			aggregate
				.SequenceID
				.ShouldBe(3);
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

			_store.Save(aggregate);

			aggregate.AddEvent(new TestEvent()); //4

			_store.Save(aggregate);

			_backing
				.LoadEvents(_store.Conventions<TestAggregate>(), aggregate.ID)
				.Last()
				.Sequence
				.ShouldBe(4);

			aggregate
				.SequenceID
				.ShouldBe(4);
		}
	}
}
