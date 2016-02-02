using System;

namespace Ledger.Tests.AggregateStoreTests.MiniDomain
{
	public class UserSnapshot : ISnapshot<Guid>
	{
		public Guid AggregateID { get; set; }
		public int Sequence { get; set; }
		public DateTime Stamp { get; set; }

		public string Name { get; set; }
	}
}