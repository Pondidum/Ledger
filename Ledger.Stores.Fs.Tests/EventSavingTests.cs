using System;
using System.IO;
using TestsDomain;
using Xunit;

namespace Ledger.Stores.Fs.Tests
{
	public class EventSavingTests : IDisposable
	{
		private const string _path = "temp-fs";

		public EventSavingTests()
		{
			if (Directory.Exists(_path))
			{
				Directory.Delete(_path, true);
			}

			Directory.CreateDirectory(_path);

			var aggregate = Candidate.Create("Cloud", "cloud.strife@ffvii.com");
			aggregate.FixName("cloud strife");
			aggregate.AddNewEmail("cloud@strife.com");

			var fs = new FileEventStore(_path);
			var store = new AggregateStore<Guid>(fs);

			store.Save(aggregate);
		}

		[Fact]
		public void Run()
		{
			
		}
		public void Dispose()
		{
			try
			{
				Directory.Delete(_path, true);
			}
			catch (Exception)
			{}
		}
	}
}
