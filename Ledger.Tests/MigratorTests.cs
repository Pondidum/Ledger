using System;
using System.Linq;
using Ledger.Acceptance;
using Ledger.Acceptance.TestObjects;
using Ledger.Infrastructure;
using Ledger.Migrations;
using Ledger.Stores;
using Shouldly;
using Xunit;

namespace Ledger.Tests
{
	public class MigratorTests
	{
		private static readonly EventStoreContext TestStream = new EventStoreContext("TestStream", Default.SerializerSettings);
		private readonly IncrementingStamper _stamper;

		public MigratorTests()
		{
			_stamper = new IncrementingStamper();
		}

		private void WriteEvents(InMemoryEventStore store, params DomainEvent<Guid>[] events)
		{
			var aggregateID = Guid.NewGuid();

			using (var writer = store.CreateWriter<Guid>(TestStream))
			{
				writer.SaveEvents(events
					.Apply(e => e.Stamp = _stamper.GetNext())
					.Apply(e => e.AggregateID = aggregateID));
			}
		}

		private void WriteSnapshots(InMemoryEventStore store, params Snapshot<Guid>[] snapshots)
		{
			var aggregateID = Guid.NewGuid();

			using (var writer = store.CreateWriter<Guid>(TestStream))
			{
				snapshots
					.Apply(s => s.Stamp = _stamper.GetNext())
					.Apply(s => s.AggregateID = aggregateID)
					.ForEach(s => writer.SaveSnapshot(s));
			}
		}

		[Fact]
		public void When_migrating_events_only_to_a_new_stream()
		{
			var source = new InMemoryEventStore();
			var dest = new InMemoryEventStore();

			WriteEvents(source, new TestEvent(), new TestEvent(), new TestEvent());

			var migrator = new Migrator(new BlankDestinationStrategy());
			migrator.ToEmptyStream<Guid>(source, dest, TestStream);

			dest.AllEvents.Count().ShouldBe(3);
		}

		[Fact]
		public void When_migrating_snapshots_only_to_a_new_stream()
		{
			var source = new InMemoryEventStore();
			var dest = new InMemoryEventStore();

			WriteSnapshots(source, new TestSnapshot());


			var migrator = new Migrator(new BlankDestinationStrategy());
			migrator.ToEmptyStream<Guid>(source, dest, TestStream);

			dest.AllSnapshots.Count().ShouldBe(1);
		}

		[Fact]
		public void When_migrating_snapshots_only_latest_is_kept()
		{
			var source = new InMemoryEventStore();
			var dest = new InMemoryEventStore();

			WriteSnapshots(source, new TestSnapshot(), new TestSnapshot(), new TestSnapshot());


			var migrator = new Migrator(new BlankDestinationStrategy());
			migrator.ToEmptyStream<Guid>(source, dest, TestStream);

			dest.AllSnapshots.Count().ShouldBe(1);
		}
	}
}
