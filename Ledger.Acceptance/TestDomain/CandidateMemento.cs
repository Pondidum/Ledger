using System;
using System.Collections.Generic;
using Ledger.Infrastructure;

namespace Ledger.Acceptance.TestDomain
{
	public class CandidateMemento : Snapshot<Guid>
	{
		public override Guid AggregateID { get; set; }
		public override Sequence Sequence { get; set; }
		public override DateTime Stamp { get; set; }

		public string Name { get; set; }
		public List<string> Emails { get; set; }
	}
}
