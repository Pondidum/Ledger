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
			CleanupTables();
			
			var create = new CreateGuidKeyedTablesCommand(ConnectionString);
			create.Execute();

			_store = new PostgresEventStore<Guid>(ConnectionString);
		}

		[Fact]
		public void Events_should_keep_types_and_be_ordered()
		{
			var toSave = new DomainEvent[]
			{
				new NameChangedByDeedPoll {SequenceID = 0, NewName = "Deed"},
				new FixNameSpelling {SequenceID = 1, NewName = "Fix"},
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

			_store.SaveEvents(first, new[] { new FixNameSpelling { SequenceID = 4 } });
			_store.SaveEvents(first, new[] { new FixNameSpelling { SequenceID = 5 } });
			_store.SaveEvents(second, new[] { new NameChangedByDeedPoll { SequenceID = 6 } });

			_store
				.GetLatestSequenceFor(first)
				.ShouldBe(5);
		}
		
		[Fact]
		public void Loading_events_since_only_gets_events_after_the_sequence()
		{
			var toSave = new DomainEvent[]
			{
				new NameChangedByDeedPoll { SequenceID = 3 },
				new FixNameSpelling { SequenceID = 4 },
				new FixNameSpelling { SequenceID = 5 },
				new FixNameSpelling { SequenceID = 6 },
			};

			var id = Guid.NewGuid();

			_store.SaveEvents(id, toSave);

			var loaded = _store.LoadEventsSince(id, 4);

			loaded.Select(x => x.SequenceID).ShouldBe(new[] { 5, 6 });
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
