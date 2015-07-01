using System;
using System.IO;
using System.Linq;
using NSubstitute;
using Shouldly;
using TestsDomain.Events;
using Xunit;

namespace Ledger.Stores.Fs.Tests
{
	public class EventSaveLoadTests : IDisposable
	{
		private readonly string _root;
		private readonly FileEventStore<Guid> _store;

		public EventSaveLoadTests()
		{
			_root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

			Directory.CreateDirectory(_root);
			_store = new FileEventStore<Guid>(_root);
		}

		[Fact]
		public void The_events_should_keep_types()
		{
			var toSave = new DomainEvent[]
			{
				new NameChangedByDeedPoll {NewName = "Deed"},
				new FixNameSpelling {NewName = "Fix"},
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
		public void When_there_is_no_event_file_and_load_is_called()
		{
			var id = Guid.NewGuid();


			var loaded = _store.LoadEventsSince(id, 4);
			
			loaded.ShouldBeEmpty();
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
