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
		public void When_getting_events_they_get_ordered()
		{
			var store = new InMemoryEventStore<int>();

			var e0 = new TestEvent { Sequence = 0 };
			var e1 = new TestEvent { Sequence = 0 };
			var e2 = new TestEvent { Sequence = 0 };

			store.SaveEvents(null, 1, new List<IDomainEvent>() { e0 });
			store.SaveEvents(null, 2, new List<IDomainEvent>() { e1 });
			store.SaveEvents(null, 1, new List<IDomainEvent>() { e2 });

			store.AllEvents.ShouldBe(new[] { e0, e1, e2 });
		}
	}
}
