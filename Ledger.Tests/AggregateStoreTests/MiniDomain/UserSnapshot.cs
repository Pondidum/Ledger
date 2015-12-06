using System;

namespace Ledger.Tests.AggregateStoreTests.MiniDomain
{
	public class UserSnapshot : ISnapshot<Guid>
	{
		public DateTime Stamp { get; set; }
		public Guid AggregateID { get; set; }

		public string Name { get; set; }
	}
}