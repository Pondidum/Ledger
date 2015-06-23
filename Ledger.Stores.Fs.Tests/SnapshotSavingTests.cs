using System;
using System.Linq;
using Ledger.Infrastructure;
using Shouldly;
using TestsDomain;
using Xunit;

namespace Ledger.Stores.Fs.Tests
{
	public class SnapshotSavingTests : TestBase
	{
		private readonly Candidate _aggregate;

		public SnapshotSavingTests()
		{
			_aggregate = Candidate.Create("test", "test@home.com");

			Enumerable
				.Range(0, 15)
				.Select(i => i.ToString())
				.ForEach(i => _aggregate.FixName(i));

			Save(_aggregate);
		}

		[Fact]
		public void There_should_be_16_events()
		{
			Events.Count.ShouldBe(16);
		}

		[Fact]
		public void There_should_be_1_snapshot()
		{
			Snapshots.Count.ShouldBe(1);
		}

		[Fact]
		public void The_snapshot_should_be()
		{
			Snapshots[0].ShouldBe("{\"ID\":\"" + _aggregate.ID + "\",\"Snapshot\":{\"SequenceID\":15,\"Name\":\"14\",\"Emails\":[\"test@home.com\"]}}");
		}
	}
}
