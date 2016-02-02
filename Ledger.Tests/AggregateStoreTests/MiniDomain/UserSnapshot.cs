using System;

namespace Ledger.Tests.AggregateStoreTests.MiniDomain
{
	public class UserSnapshot : Snapshot<Guid>
	{
		public override Guid AggregateID { get; set; }
		public override int Sequence { get; set; }
		public override DateTime Stamp { get; set; }

		public string Name { get; set; }
	}
}