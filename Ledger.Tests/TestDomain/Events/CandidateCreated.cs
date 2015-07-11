using System;

namespace Ledger.Tests.TestDomain.Events
{
	public class CandidateCreated : DomainEvent
	{
		public Guid CandidateID { get; set; }
		public string CandidateName { get; set; }
		public string EmailAddress { get; set; }
	}
}
