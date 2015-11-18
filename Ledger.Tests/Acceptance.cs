using Ledger.Acceptance;
using Ledger.Stores;
using Xunit;

namespace Ledger.Tests
{
	public class Acceptance : AcceptanceTests
	{
		public Acceptance() 
			: base(new InMemoryEventStore())
		{
		}
	}
}
