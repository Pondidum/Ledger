using System;
using Ledger.Tests.TestObjects;
using Shouldly;
using Xunit;

namespace Ledger.Tests.AcceptanceTests
{
	public class LoadingMultipleEventsWithoutSnapshotting : AcceptanceBase<TestAggregate>
	{
		public LoadingMultipleEventsWithoutSnapshotting()
		{
			var aggregateStore = new AggregateStore<Guid>(EventStore);
			var id = Guid.NewGuid();

			EventStore.SaveEvents(id, new[] 
			{
				new TestEvent { SequenceID = 5},
				new TestEvent { SequenceID = 6},
			});

			Aggregate = aggregateStore.Load(id, () => new TestAggregate());
		}

		[Fact]
		public void The_uncommitted_changes_should_be_empty()
		{
			Aggregate.GetUncommittedEvents().ShouldBeEmpty();
		}

		[Fact]
		public void The_sequence_id_should_be_the_last_events()
		{
			Aggregate.SequenceID.ShouldBe(6);
		}
	}
}
