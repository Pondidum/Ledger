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
			var store = new FileEventStore(_root);

			store.SaveSnapshot(id, new CandidateMemento());

			var loaded = store.GetLatestSnapshotFor(id);

			loaded.ShouldBeOfType<CandidateMemento>();
		}

		[Fact]
		public void Only_the_latest_snapshot_should_be_loaded()
		{
			var id = Guid.NewGuid();
			var store = new FileEventStore(_root);

			store.SaveSnapshot(id, new CandidateMemento { SequenceID = 4 });
			store.SaveSnapshot(id, new CandidateMemento { SequenceID = 5 });

			store
				.GetLatestSnapshotFor(id)
				.SequenceID
				.ShouldBe(5);
		}

		[Fact]
		public void The_most_recent_snapshot_id_should_be_found()
		{
			var id = Guid.NewGuid();
			var store = new FileEventStore(_root);

			store.SaveSnapshot(id, new CandidateMemento { SequenceID = 4 });
			store.SaveSnapshot(id, new CandidateMemento { SequenceID = 5 });

			store
				.GetLatestSnapshotIDFor(id)
				.ShouldBe(5);
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
