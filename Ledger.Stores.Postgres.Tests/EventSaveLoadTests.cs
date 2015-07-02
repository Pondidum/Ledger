using System;
using System.Linq;
using Shouldly;
using TestsDomain.Events;
using Xunit;

namespace Ledger.Stores.Postgres.Tests
{
	public class EventSaveLoadTests : PostgresTestBase
	{
		private readonly PostgresEventStore<Guid> _store;

		public EventSaveLoadTests()
		{
			_store = new PostgresEventStore<Guid>(ConnectionString);
		}

		[Fact]
		public void Events_should_keep_types_and_be_ordered()
		{
			var toSave = new DomainEvent[]
			{
				new NameChangedByDeedPoll {Sequence = 0, NewName = "Deed"},
				new FixNameSpelling {Sequence = 1, NewName = "Fix"},
			};

			var id = Guid.NewGuid();
			_store.SaveEvents(id, toSave);

			var loaded = _store.LoadEvents(id);

			loaded.First().ShouldBeOfType<NameChangedByDeedPoll>();
			loaded.Last().ShouldBeOfType<FixNameSpelling>();
		}

		[Fact]
		public void Only_events_for_the_correct_aggregate_are_returned()
		{
			var first = Guid.NewGuid();
			var second = Guid.NewGuid();

			_store.SaveEvents(first, new[] { new FixNameSpelling { NewName = "Fix" } });
			_store.SaveEvents(second, new[] { new NameChangedByDeedPoll { NewName = "Deed" } });

			var loaded = _store.LoadEvents(first);

			loaded.Single().ShouldBeOfType<FixNameSpelling>();
		}

		[Fact]
		public void Only_the_latest_sequence_is_returned()
		{
			var first = Guid.NewGuid();
			var second = Guid.NewGuid();

			_store.SaveEvents(first, new[] { new FixNameSpelling { Sequence = 4 } });
			_store.SaveEvents(first, new[] { new FixNameSpelling { Sequence = 5 } });
			_store.SaveEvents(second, new[] { new NameChangedByDeedPoll { Sequence = 6 } });

			_store
				.GetLatestSequenceFor(first)
				.ShouldBe(5);
		}
		
		[Fact]
		public void Loading_events_since_only_gets_events_after_the_sequence()
		{
			var toSave = new DomainEvent[]
			{
				new NameChangedByDeedPoll { Sequence = 3 },
				new FixNameSpelling { Sequence = 4 },
				new FixNameSpelling { Sequence = 5 },
				new FixNameSpelling { Sequence = 6 },
			};

			var id = Guid.NewGuid();

			_store.SaveEvents(id, toSave);

			var loaded = _store.LoadEventsSince(id, 4);

			loaded.Select(x => x.Sequence).ShouldBe(new[] { 5, 6 });
		}

		[Fact]
		public void When_there_are_no_events_and_load_is_called()
		{
			var id = Guid.NewGuid();


			var loaded = _store.LoadEventsSince(id, 4);

			loaded.ShouldBeEmpty();
		}
	}
}
