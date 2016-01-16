using System;
using System.Collections.Generic;
using System.Linq;
using Ledger.Acceptance;
using Ledger.Acceptance.TestObjects;
using Ledger.Infrastructure;
using Ledger.Stores;
using Ledger.Tests.AggregateStoreTests.MiniDomain;
using Ledger.Tests.AggregateStoreTests.MiniDomain.Events;
using Shouldly;
using Xunit;

namespace Ledger.Tests
{
	public class SaveAggregateTests
	{
		private const string StreamName = "someStream";

		private readonly InMemoryEventStore _backing;
		private readonly AggregateStore<Guid> _store;
		private IncrementingStamper _stamper;

		public SaveAggregateTests()
		{
			_backing = new InMemoryEventStore();
			_store = new AggregateStore<Guid>(_backing);

			_stamper = new IncrementingStamper();
		}

		[Fact]
		public void When_a_single_event_is_saved()
		{
			var aggregate = new TestAggregate(_stamper.GetNext);
			aggregate.GenerateID();

			aggregate.AddEvent(new TestEvent());

			_store.Save(StreamName, aggregate);

			var events = _backing
				.CreateReader<Guid>(StreamName)
				.LoadEvents(aggregate.ID)
				.ToList();

			aggregate.ShouldSatisfyAllConditions(
				() => events.ShouldAllBe(e => e.AggregateID == aggregate.ID),
				() => events.ForEach((e, i) => e.Stamp.ShouldBe(_stamper.Offset(i))),
				() => aggregate.GetSequenceID().ShouldBe(_stamper.Start)
            );

		}

		[Fact]
		public void When_multiple_events_are_saved()
		{
			var aggregate = new TestAggregate(_stamper.GetNext);
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
				() => events.ForEach((e, i) => e.Stamp.ShouldBe(_stamper.Offset(i))),
				() => aggregate.GetSequenceID().ShouldBe(_stamper.Offset(3))
			);
		}


		[Fact]
		public void When_an_event_is_saved_onto_a_saved_aggregate()
		{
			var aggregate = new TestAggregate(_stamper.GetNext);
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
				() => events.ForEach((e, i) => e.Stamp.ShouldBe(_stamper.Offset(i))),
				() => aggregate.GetSequenceID().ShouldBe(_stamper.Offset(4)),
				() => events.Count.ShouldBe(5)
			);
		}

		[Fact]
		public void When_the_aggregate_implements_another_interface()
		{
			var aggregate = new InterfaceAggregate(_stamper.GetNext);
			aggregate.GenerateID();

			aggregate.AddEvent(new TestEvent());

			_store.Save(StreamName, aggregate);

			var events = _backing
				.CreateReader<Guid>(StreamName)
				.LoadEvents(aggregate.ID)
				.ToList();

			aggregate.ShouldSatisfyAllConditions(
				() => events.ShouldAllBe(e => e.AggregateID == aggregate.ID),
				() => events.ForEach((e, i) => e.Stamp.ShouldBe(_stamper.Offset(i))),
				() => aggregate.GetSequenceID().ShouldBe(_stamper.Offset(0))
			);
		}

		[Fact]
		public void When_the_aggregate_is_snapshottable()
		{
			var aggregate = new SnapshotAggregate(_stamper.GetNext);
			aggregate.GenerateID();

			Enumerable.Range(0, 12).ForEach((e,i) => aggregate.AddEvent(new TestEvent()));

			_store.Save(StreamName, aggregate);

			using (var reader = _backing.CreateReader<Guid>(StreamName))
			{
				var events = reader.LoadEvents(aggregate.ID).ToList();
				var snapshot = reader.LoadLatestSnapshotFor(aggregate.ID);

				aggregate.ShouldSatisfyAllConditions(
					() => events.Count.ShouldBe(12),
					() => snapshot.Stamp.ShouldBe(_stamper.Offset(11)),
					() => snapshot.AggregateID.ShouldBe(aggregate.ID)
				);
			}
		}

		[Fact]
		public void When_the_aggregate_needs_a_new_snapshot()
		{
			var aggregate = new SnapshotAggregate(_stamper.GetNext);
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
					() => snapshot.Stamp.ShouldBe(_stamper.Offset(23)),
					() => snapshot.AggregateID.ShouldBe(aggregate.ID)
				);
			}
		}

		[Fact]
		public void When_a_few_events_push_over_the_snapshot_threashold()
		{
			var aggregate = new SnapshotAggregate(_stamper.GetNext);
			aggregate.GenerateID();

			Enumerable.Range(0, _store.SnapshotPolicy.DefaultInterval - 2).ForEach(e => aggregate.AddEvent(new TestEvent()));
			_store.Save(StreamName, aggregate);

			Enumerable.Range(0, 3).ForEach(e => aggregate.AddEvent(new TestEvent()));
			_store.Save(StreamName, aggregate);

			using (var reader = _backing.CreateReader<Guid>(StreamName))
			{
				var events = reader.LoadEvents(aggregate.ID).ToList();
				var snapshot = reader.LoadLatestSnapshotFor(aggregate.ID);

				aggregate.ShouldSatisfyAllConditions(
					() => events.Count.ShouldBe(11),
					() => snapshot.Stamp.ShouldBe(_stamper.Offset(10))
				);
			}
		}

		[Fact]
		public void When_saving_multiple_aggregates_events_stay_in_actioned_order()
		{
			var perm = Permission.Create();
			var role = Role.Create();
			perm.ChangeName("Test");
			role.ChangeName("Testing");

			_store.Save(StreamName, role);
			_store.Save(StreamName, perm);

			_backing.AllEvents.Select(e => e.GetType()).ShouldBe(new []
			{
				typeof(PermissionCreatedEvent),
				typeof(RoleCreatedEvent),
				typeof(PermissionNameChangedEvent),
				typeof(RoleNameChangedEvent)
			});

		}


		public interface IKeyed
		{
			string Key { get; }
		}

		public class InterfaceAggregate : AggregateRoot<Guid>, IKeyed
		{
			public string Key { get; set; }

			public InterfaceAggregate(Func<DateTime> getTimestamp)
				: base(getTimestamp)
			{
				
			}
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

			public DateTime GetSequenceID()
			{
				return SequenceID;
			}
		}
	}
}
