using System;
using Ledger.Tests.TestObjects;
using Shouldly;
using Xunit;

namespace Ledger.Tests.AcceptanceTests
{
	public class AggregateStoreTests
	{
		private readonly FakeEventStore _eventStore;
		private readonly AggregateStore<Guid> _aggregateStore;

		public AggregateStoreTests()
		{
			_eventStore = new FakeEventStore();
			_aggregateStore = new AggregateStore<Guid>(_eventStore);
		}

		[Fact]
		public void When_there_are_no_events()
		{
			_eventStore.LatestSequenceID = null;

			_aggregateStore.Save(new TestAggregate());

			_eventStore.Events.ShouldBeEmpty();
		}

		[Fact]
		public void When_the_event_store_has_newer_events()
		{
			_eventStore.LatestSequenceID = 5;

			Should.Throw<Exception>(() => _aggregateStore.Save(new TestAggregate()));
		}
	}
}
