using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NSubstitute;
using Shouldly;
using TestsDomain;
using Xunit;

namespace Ledger.Stores.Fs.Tests
{
	public class EventSavingTests
	{
		private readonly List<string> _events;
		private readonly List<string> _snapshots;
		private Candidate _aggregate;

		public EventSavingTests()
		{
			var events = new MemoryStream();
			var snapshots = new MemoryStream();

			var fileSystem = Substitute.For<IFileSystem>();
			fileSystem.AppendTo("fs\\Guid.events.json").Returns(events);
			fileSystem.AppendTo("fs\\Guid.snapshots.json").Returns(snapshots);

			_aggregate = Candidate.Create("Cloud", "cloud.strife@ffvii.com");
			_aggregate.FixName("cloud strife");
			_aggregate.AddNewEmail("cloud@strife.com");

			var fs = new FileEventStore(fileSystem, "fs");
			var store = new AggregateStore<Guid>(fs);

			store.Save(_aggregate);

			_events = FromStream(events);
			_snapshots = FromStream(snapshots);
		}

		private static List<string> FromStream(MemoryStream stream)
		{
			var copy = new MemoryStream(stream.ToArray());

			using (var reader = new StreamReader(copy))
			{
				return reader
					.ReadToEnd()
					.Split(new string[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries)
					.ToList();
			}
		}

		[Fact]
		public void Three_events_should_be_written()
		{
			_events.Count().ShouldBe(3);
		}

		[Fact]
		public void The_first_event_should_be()
		{
			_events[0].ShouldBe("{\"ID\":\"" + _aggregate.ID + "\",\"Event\":{\"CandidateID\":\"" + _aggregate.ID + "\",\"CandidateName\":\"Cloud\",\"EmailAddress\":\"cloud.strife@ffvii.com\",\"SequenceID\":0}}");
		}

		[Fact]
		public void The_second_event_should_be()
		{
			_events[1].ShouldBe("{\"ID\":\"" + _aggregate.ID + "\",\"Event\":{\"NewName\":\"cloud strife\",\"SequenceID\":1}}");
		}

		[Fact]
		public void The_third_event_should_be()
		{
			_events[2].ShouldBe("{\"ID\":\"" + _aggregate.ID + "\",\"Event\":{\"Email\":\"cloud@strife.com\",\"SequenceID\":2}}");
		}

		[Fact]
		public void There_should_be_no_snapshot()
		{
			_snapshots.ShouldBeEmpty();
		}
	}
}
