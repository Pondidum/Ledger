using System;
using System.Collections.Generic;

namespace Ledger.Acceptance.TestDomain
{
	public class CandidateMemento : ISnapshot<Guid>
	{
		public Guid AggregateID { get; set; }
		public int Sequence { get; set; }

		public string Name { get; set; }
		public List<string> Emails { get; set; }
	}
}
