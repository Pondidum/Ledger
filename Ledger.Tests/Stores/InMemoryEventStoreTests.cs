using System.Collections.Generic;
using Ledger.Acceptance.TestObjects;
using Ledger.Stores;
using Shouldly;
using Xunit;

namespace Ledger.Tests.Stores
{
	public class InMemoryEventStoreTests
	{
		[Fact]
		public void When_getting_events_they_are_ordered()
		{
			var store = new InMemoryEventStore<int>();

			var e0 = new TestEvent { Sequence = 0 };
			var e1 = new TestEvent { Sequence = 1 };
			var e2 = new TestEvent { Sequence = 2 };

			store.SaveEvents(null, 1, new List<IDomainEvent>() { e0 });
			store.SaveEvents(null, 2, new List<IDomainEvent>() { e1 });
			store.SaveEvents(null, 1, new List<IDomainEvent>() { e2 });

			store.AllEvents.ShouldBe(new[] { e0, e1, e2 });
		}

		[Fact]
		public void When_getting_snapshots_they_are_ordered()
		{
			var store = new InMemoryEventStore<int>();

			var snap0 = new TestSnapshot { Sequence = 0 };
			var snap1 = new TestSnapshot { Sequence = 1 };
			var snap2 = new TestSnapshot { Sequence = 2 };

			store.SaveSnapshot(null, 1, snap0);
			store.SaveSnapshot(null, 2, snap1);
			store.SaveSnapshot(null, 1, snap2);

			store.AllSnapshots.ShouldBe(new[] { snap0, snap1, snap2 });
		}
	}
}
