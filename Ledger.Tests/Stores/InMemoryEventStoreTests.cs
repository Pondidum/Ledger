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
			var store = new InMemoryEventStore();

			var e0 = new TestEvent { Sequence = 0 };
			var e1 = new TestEvent { Sequence = 1 };
			var e2 = new TestEvent { Sequence = 2 };

			using (var writer = store.CreateWriter<int>(null))
			{
				writer.SaveEvents(1, new List<IDomainEvent>() {e0});
				writer.SaveEvents(2, new List<IDomainEvent>() {e1});
				writer.SaveEvents(1, new List<IDomainEvent>() {e2});
			}

			store.AllEvents.ShouldBe(new[] { e0, e1, e2 });
		}

		[Fact]
		public void When_getting_snapshots_they_are_ordered()
		{
			var store = new InMemoryEventStore();

			var snap0 = new TestSnapshot { Sequence = 0 };
			var snap1 = new TestSnapshot { Sequence = 1 };
			var snap2 = new TestSnapshot { Sequence = 2 };

			using (var writer = store.CreateWriter<int>(null))
			{
				writer.SaveSnapshot(1, snap0);
				writer.SaveSnapshot(2, snap1);
				writer.SaveSnapshot(1, snap2);
			}

			store.AllSnapshots.ShouldBe(new[] { snap0, snap1, snap2 });
		}
	}
}
