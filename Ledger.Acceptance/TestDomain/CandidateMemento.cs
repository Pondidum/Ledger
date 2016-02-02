using System;
using System.Collections.Generic;

namespace Ledger.Acceptance.TestDomain
{
	public class CandidateMemento : Snapshot<Guid>
	{
		public override Guid AggregateID { get; set; }
		public override int Sequence { get; set; }
		public override DateTime Stamp { get; set; }

		public string Name { get; set; }
		public List<string> Emails { get; set; }
	}
}
