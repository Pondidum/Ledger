using System;
using System.Linq;
using Ledger.Acceptance.TestObjects;
using Ledger.Infrastructure;
using Ledger.Stores;
using Shouldly;
using Xunit;

namespace Ledger.Tests
{
	public class ProjectionResumeTests
	{
		[Fact]
		public void When_reading()
		{
			var ms = new InMemoryEventStore();
			var id = Guid.NewGuid();

			var context = new EventStoreContext("Test", new DefaultTypeResolver());

			using (var writer = ms.CreateWriter<Guid>(context))
			{
				writer.SaveEvents(Enumerable
					.Range(0, 50)
					.Select(i => new TestEvent { AggregateID = id, Sequence = new Sequence(i) }));
			}

			var lastSeen = 20;

			using (var reader = ms.CreateReader<Guid>(context))
			{
				var events = reader.LoadAllEventsSince(new StreamSequence(lastSeen));
				events.First().StreamSequence.ShouldBe(new StreamSequence(lastSeen + 1));
			}
		}
	}
}
