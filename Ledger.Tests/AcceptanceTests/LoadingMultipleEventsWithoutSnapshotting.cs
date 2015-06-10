using System;
using System.Collections.Generic;
using Ledger.Tests.TestObjects;
using Shouldly;
using Xunit;

namespace Ledger.Tests.AcceptanceTests
{
	public class LoadingMultipleEventsWithoutSnapshotting
	{
		private readonly TestAggregate _aggregate;

		public LoadingMultipleEventsWithoutSnapshotting()
		{
			var eventStore = new FakeEventStore();
			var aggregateStore = new AggregateStore<Guid>(eventStore);

			eventStore.ReadFromEvents = new List<object>
			{
				new TestEvent { SequenceID = 5},
				new TestEvent { SequenceID = 6},
			};

			_aggregate = aggregateStore.Load(Guid.NewGuid(), () => new TestAggregate());
		}

		[Fact]
		public void The_uncommitted_changes_should_be_empty()
		{
			_aggregate.GetUncommittedEvents().ShouldBeEmpty();
		}

		[Fact]
		public void The_sequence_id_should_be_the_last_events()
		{
			_aggregate.SequenceID.ShouldBe(6);
		}
	}
}
