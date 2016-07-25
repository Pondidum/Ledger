using System;
using Ledger.Infrastructure;

namespace Ledger.Tests.AggregateStoreTests.MiniDomain
{
	public class UserSnapshot : Snapshot<Guid>
	{
		public override Guid AggregateID { get; set; }
		public override Sequence Sequence { get; set; }
		public override DateTime Stamp { get; set; }

		public string Name { get; set; }
	}
}