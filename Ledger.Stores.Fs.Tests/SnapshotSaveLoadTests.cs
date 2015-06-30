using System;
using System.IO;
using Shouldly;
using TestsDomain;
using Xunit;

namespace Ledger.Stores.Fs.Tests
{
	public class SnapshotSaveLoadTests : IDisposable
	{
		private readonly string _root;

		public SnapshotSaveLoadTests()
		{
			_root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

			Directory.CreateDirectory(_root);
		}

		[Fact]
		public void A_snapshot_should_maintain_type()
		{
			var id = Guid.NewGuid();
			var store = new FileEventStore<Guid>(_root);

			store.SaveSnapshot(id, new CandidateMemento());

			var loaded = store.LoadLatestSnapshotFor(id);

			loaded.ShouldBeOfType<CandidateMemento>();
		}

		[Fact]
		public void Only_the_latest_snapshot_should_be_loaded()
		{
			var id = Guid.NewGuid();
			var store = new FileEventStore<Guid>(_root);

			store.SaveSnapshot(id, new CandidateMemento { SequenceID = 4 });
			store.SaveSnapshot(id, new CandidateMemento { SequenceID = 5 });

			store
				.LoadLatestSnapshotFor(id)
				.SequenceID
				.ShouldBe(5);
		}

		[Fact]
		public void The_most_recent_snapshot_id_should_be_found()
		{
			var id = Guid.NewGuid();
			var store = new FileEventStore<Guid>(_root);

			store.SaveSnapshot(id, new CandidateMemento { SequenceID = 4 });
			store.SaveSnapshot(id, new CandidateMemento { SequenceID = 5 });

			store
				.GetLatestSnapshotSequenceFor(id)
				.ShouldBe(5);
		}


		[Fact]
		public void When_there_is_no_snapshot_file_and_load_is_called()
		{
			var id = Guid.NewGuid();
			var store = new FileEventStore<Guid>(_root);

			var loaded = store.LoadLatestSnapshotFor(id);

			loaded.ShouldBe(null);
		}

		public void Dispose()
		{
			try
			{
				Directory.Delete(_root, true);
			}
			catch (Exception)
			{
			}
		}
	}
}
