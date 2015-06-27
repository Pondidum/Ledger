using Ledger.Tests.TestObjects;

namespace Ledger.Tests.AcceptanceTests
{
	public class AcceptanceBase<TAggregate>
	{
		public TAggregate Aggregate { get; set; }
		public IEventStore EventStore { get; set; }

		public AcceptanceBase()
		{
			EventStore = new FakeEventStore();
		}
	}
}
