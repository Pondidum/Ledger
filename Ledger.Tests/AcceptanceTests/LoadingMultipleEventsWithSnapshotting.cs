using System;
using System.Collections.Generic;
using Ledger.Tests.TestObjects;
using Shouldly;
using Xunit;

namespace Ledger.Tests.AcceptanceTests
{
	public class LoadingMultipleEventsWithSnapshotting
	{
		private readonly SnapshotAggregate _aggregate;

		public LoadingMultipleEventsWithSnapshotting()
		{
			var eventStore = new FakeEventStore();
			var aggregateStore = new AggregateStore<Guid>(eventStore);

			eventStore.Snapshot = new TestSnapshot() {SequenceID = 10};
			eventStore.ReadFromEvents = new List<object>
			{
				new TestEvent { SequenceID = 5},
				new TestEvent { SequenceID = 6},
			};

			_aggregate = aggregateStore.Load<SnapshotAggregate, TestSnapshot>(Guid.NewGuid(), () => new SnapshotAggregate());
		}

		[Fact]
		public void The_uncommitted_changes_should_be_empty()
		{
			_aggregate.GetUncommittedEvents().ShouldBeEmpty();
		}

		[Fact]
		public void The_sequence_id_should_be_the_last_events()
		{
			_aggregate.SequenceID.ShouldBe(10);
		}

	}
}
