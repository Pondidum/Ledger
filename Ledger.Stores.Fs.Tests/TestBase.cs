using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ledger.Infrastructure;
using NSubstitute;
using TestsDomain;

namespace Ledger.Stores.Fs.Tests
{
	public class TestBase
	{
		public List<string> Events { get; private set; }
		public List<string> Snapshots { get; private set; }

		public TestBase()
		{
			Events = new List<string>();
			Snapshots = new List<string>();
		}

		protected void Save(AggregateRoot<Guid> aggregate)
		{
			var events = new MemoryStream();
			var snapshots = new MemoryStream();

			var fileSystem = Substitute.For<IFileSystem>();
			fileSystem.AppendTo("fs\\Guid.events.json").Returns(events);
			fileSystem.AppendTo("fs\\Guid.snapshots.json").Returns(snapshots);

			var fs = new FileEventStore(fileSystem, "fs");
			var store = new AggregateStore<Guid>(fs);

			store.Save(aggregate);

			Events.Clear();
			Events.AddRange(FromStream(events));

			Snapshots.Clear();
			Snapshots.AddRange(FromStream(snapshots));
		}

		protected Candidate Load(Guid id)
		{
			var events = ToStream(Events);
			var snapshots = ToStream(Snapshots);

			var fileSystem = Substitute.For<IFileSystem>();
			fileSystem.ReadFile("fs\\Guid.events.json").Returns(events);
			fileSystem.ReadFile("fs\\Guid.snapshots.json").Returns(snapshots);
			fileSystem.FileExists(Arg.Any<string>()).Returns(true);

			var fs = new FileEventStore(fileSystem, "fs");
			var store = new AggregateStore<Guid>(fs);

			return store.Load(id, () => new Candidate());
		}

		private static List<string> FromStream(MemoryStream stream)
		{
			var copy = new MemoryStream(stream.ToArray());

			using (var reader = new StreamReader(copy))
			{
				return reader
					.ReadToEnd()
					.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
					.ToList();
			}
		}

		private static Stream ToStream(IEnumerable<string> items)
		{
			var ms = new MemoryStream();
			var sw = new StreamWriter(ms);

			items.ForEach(x => sw.WriteLine(x));

			sw.Flush();
			ms.Position = 0;

			return ms;
		}

	}
}
