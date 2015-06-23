using System.Text;
using Shouldly;
using TestsDomain;
using Xunit;

namespace Ledger.Stores.Fs.Tests
{
	public class EventSavingTests : TestBase
	{
		private Candidate _aggregate;

		public EventSavingTests()
		{
			_aggregate = Candidate.Create("Cloud", "cloud.strife@ffvii.com");
			_aggregate.FixName("cloud strife");
			_aggregate.AddNewEmail("cloud@strife.com");

			Save(_aggregate);
		}

		[Fact]
		public void Three_events_should_be_written()
		{
			Events.Count.ShouldBe(3);
		}

		[Fact]
		public void The_first_event_should_be()
		{
			Events[0].ShouldBe("{\"ID\":\"" + _aggregate.ID + "\",\"Event\":{\"CandidateID\":\"" + _aggregate.ID + "\",\"CandidateName\":\"Cloud\",\"EmailAddress\":\"cloud.strife@ffvii.com\",\"SequenceID\":0}}");
		}

		[Fact]
		public void The_second_event_should_be()
		{
			Events[1].ShouldBe("{\"ID\":\"" + _aggregate.ID + "\",\"Event\":{\"NewName\":\"cloud strife\",\"SequenceID\":1}}");
		}

		[Fact]
		public void The_third_event_should_be()
		{
			Events[2].ShouldBe("{\"ID\":\"" + _aggregate.ID + "\",\"Event\":{\"Email\":\"cloud@strife.com\",\"SequenceID\":2}}");
		}

		[Fact]
		public void There_should_be_no_snapshot()
		{
			Snapshots.ShouldBeEmpty();
		}
	}
}
