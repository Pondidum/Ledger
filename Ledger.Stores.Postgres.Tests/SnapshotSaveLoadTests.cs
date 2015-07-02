using System;
using System.IO;
using Shouldly;
using TestsDomain;
using Xunit;

namespace Ledger.Stores.Postgres.Tests
{
	public class SnapshotSaveLoadTests : PostgresTestBase
	{
		private readonly PostgresEventStore<Guid> _store;

		public SnapshotSaveLoadTests()
		{
			CleanupTables();

			var create = new CreateGuidKeyedTablesCommand(ConnectionString);
			create.Execute();

			_store = new PostgresEventStore<Guid>(ConnectionString);
		}

		[Fact]
		public void A_snapshot_should_maintain_type()
		{
			var id = Guid.NewGuid();

			_store.SaveSnapshot(id, new CandidateMemento{Sequence = 0});
			_store.SaveSnapshot(id, new CandidateMemento { Sequence = 1 });
			_store.SaveSnapshot(id, new CandidateMemento { Sequence = 2 });
			_store.SaveSnapshot(id, new CandidateMemento { Sequence = 3 });

			var loaded = _store.LoadLatestSnapshotFor(id);

			loaded.ShouldBeOfType<CandidateMemento>();
		}

		[Fact]
		public void Only_the_latest_snapshot_should_be_loaded()
		{
			var id = Guid.NewGuid();

			_store.SaveSnapshot(id, new CandidateMemento { Sequence = 4 });
			_store.SaveSnapshot(id, new CandidateMemento { Sequence = 5 });

			_store
				.LoadLatestSnapshotFor(id)
				.Sequence
				.ShouldBe(5);
		}

		[Fact]
		public void The_most_recent_snapshot_id_should_be_found()
		{
			var id = Guid.NewGuid();

			_store.SaveSnapshot(id, new CandidateMemento { Sequence = 4 });
			_store.SaveSnapshot(id, new CandidateMemento { Sequence = 5 });

			_store
				.GetLatestSnapshotSequenceFor(id)
				.ShouldBe(5);
		}


		[Fact]
		public void When_there_is_no_snapshot_file_and_load_is_called()
		{
			var id = Guid.NewGuid();

			var loaded = _store.LoadLatestSnapshotFor(id);

			loaded.ShouldBe(null);
		}
	}
}
