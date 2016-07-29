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
		private readonly Guid _aggregateID;
		private readonly InMemoryEventStore _store;
		private readonly EventStoreContext _context;

		public ProjectionResumeTests()
		{
			_aggregateID = Guid.NewGuid();
			_store = new InMemoryEventStore();
			_context = new EventStoreContext("Test", new DefaultTypeResolver());

			using (var writer = _store.CreateWriter<Guid>(_context))
			{
				writer.SaveEvents(Enumerable
					.Range(0, 50)
					.Select(i => new TestEvent { AggregateID = _aggregateID, Sequence = new Sequence(i) }));
			}
		}

		[Fact]
		public void When_reading_from_the_beginning()
		{
			using (var reader = _store.CreateReader<Guid>(_context))
			{
				var events = reader.LoadAllEventsSince(StreamSequence.Start);
				events.First().StreamSequence.ShouldBe(new StreamSequence(0));
			}
		}

		[Fact]
		public void When_reading_from_a_random_point()
		{
			var lastSeen = 20;

			using (var reader = _store.CreateReader<Guid>(_context))
			{
				var events = reader.LoadAllEventsSince(new StreamSequence(lastSeen));
				events.First().StreamSequence.ShouldBe(new StreamSequence(lastSeen + 1));
			}
		}
	}
}
