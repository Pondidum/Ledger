using System;
using System.Collections.Generic;
using System.Linq;
using Ledger.Stores;
using Shouldly;
using Xunit;

namespace Ledger.Tests.Stores
{
	public class InMemoryEventStoreTests
	{
		private readonly DateTime _start;
		private readonly Func<DateTime> _stamper;

		public InMemoryEventStoreTests()
		{
			var offset = 0;
			_start = DateTime.UtcNow;
			_stamper = new Func<DateTime>(() => _start.AddSeconds(offset++));
		}

		[Fact]
		public void When_getting_events_they_are_ordered()
		{
			var store = new InMemoryEventStore();

			var e0 = new TestEvent { AggregateID = 1, Stamp = _stamper() };
			var e1 = new TestEvent { AggregateID = 2, Stamp = _stamper() };
			var e2 = new TestEvent { AggregateID = 1, Stamp = _stamper() };

			using (var writer = store.CreateWriter<int>(null))
			{
				writer.SaveEvents(new List<IDomainEvent<int>>() { e0 });
				writer.SaveEvents(new List<IDomainEvent<int>>() { e1 });
				writer.SaveEvents(new List<IDomainEvent<int>>() { e2 });
			}

			store.AllEvents.Cast<IDomainEvent<int>>().ShouldBe(new[] { e0, e1, e2 });
		}

		[Fact]
		public void When_reading_all_events_they_are_ordrered()
		{
			var store = new InMemoryEventStore();

			var e0 = new TestEvent { AggregateID = 1, Stamp = _stamper() };
			var e1 = new TestEvent { AggregateID = 2, Stamp = _stamper() };
			var e2 = new TestEvent { AggregateID = 1, Stamp = _stamper() };

			using (var writer = store.CreateWriter<int>(null))
			{
				writer.SaveEvents(new List<IDomainEvent<int>>() { e0 });
				writer.SaveEvents(new List<IDomainEvent<int>>() { e1 });
				writer.SaveEvents(new List<IDomainEvent<int>>() { e2 });
			}

			using (var reader = store.CreateReader<int>(null))
			{
				reader.LoadAllEvents().ShouldBe(new[] { e0, e1, e2 });
			}
		}

		[Fact]
		public void When_getting_snapshots_they_are_ordered()
		{
			var store = new InMemoryEventStore();

			var snap0 = new TestSnapshot { AggregateID = 1, Stamp = _stamper() };
			var snap1 = new TestSnapshot { AggregateID = 2, Stamp = _stamper() };
			var snap2 = new TestSnapshot { AggregateID = 1, Stamp = _stamper() };

			using (var writer = store.CreateWriter<int>(null))
			{
				writer.SaveSnapshot(snap0);
				writer.SaveSnapshot(snap1);
				writer.SaveSnapshot(snap2);
			}

			store.AllSnapshots.ShouldBe(new[] { snap0, snap1, snap2 });
		}

		public class TestEvent : DomainEvent<int>
		{
		}

		public class TestSnapshot : ISnapshot<int>
		{
			public int AggregateID { get; set; }
			public DateTime Stamp { get; set; }
		}
	}
}
