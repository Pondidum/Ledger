﻿using System;
using System.Linq;
using Shouldly;
using TestsDomain.Events;
using Xunit;

namespace Ledger.Stores.Postgres.Tests
{
	public class EventSaveLoadTests : PostgresTestBase
	{
		public EventSaveLoadTests()
		{
			CleanupTables();
			var create = new CreateGuidKeyedTablesCommand(ConnectionString);
			create.Execute();
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
			var store = new PostgresEventStore<Guid>(ConnectionString);
			store.SaveEvents(id, toSave);

			var loaded = store.LoadEvents(id);

			loaded.First().ShouldBeOfType<NameChangedByDeedPoll>();
			loaded.Last().ShouldBeOfType<FixNameSpelling>();
		}

		[Fact]
		public void Only_events_for_the_correct_aggregate_are_returned()
		{
			var first = Guid.NewGuid();
			var second = Guid.NewGuid();

			var store = new PostgresEventStore<Guid>(ConnectionString);
			store.SaveEvents(first, new[] { new FixNameSpelling { NewName = "Fix" } });
			store.SaveEvents(second, new[] { new NameChangedByDeedPoll { NewName = "Deed" } });

			var loaded = store.LoadEvents(first);

			loaded.Single().ShouldBeOfType<FixNameSpelling>();
		}
	}
}
