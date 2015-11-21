using System.Collections.Generic;

namespace Ledger.Acceptance.TestDomain
{
	public class CandidateMemento : ISnapshot
	{
		public int Sequence { get; set; }

		public string Name { get; set; }
		public List<string> Emails { get; set; }
	}
}
