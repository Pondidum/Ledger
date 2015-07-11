using System.Collections.Generic;

namespace Ledger.Tests.TestDomain
{
	public class CandidateMemento : ISequenced
	{
		public int Sequence { get; set; }

		public string Name { get; set; }
		public List<string> Emails { get; set; }
	}
}
