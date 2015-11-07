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

		[Fact]
		public void When_the_aggregate_implements_another_interface()
		{
			var aggregate = new InterfaceAggregate();
			aggregate.GenerateID();

			aggregate.AddEvent(new TestEvent());

			_store.Save(aggregate);

			_backing
				.LoadEvents(_store.Conventions<InterfaceAggregate>(), aggregate.ID)
				.Last()
				.Sequence
				.ShouldBe(0);

			aggregate
				.SequenceID
				.ShouldBe(0);
		}

		public interface IKeyed
		{
			string Key { get; }
		}

		public class InterfaceAggregate : AggregateRoot<Guid>, IKeyed
		{
			public string Key { get; set; }

			public void AddEvent(DomainEvent @event)
			{
				ApplyEvent(@event);
			}

			public void AddEvents(IEnumerable<DomainEvent> events)
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
