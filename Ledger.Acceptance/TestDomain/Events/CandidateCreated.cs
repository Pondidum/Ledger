﻿using System;

namespace Ledger.Acceptance.TestDomain.Events
{
	public class CandidateCreated : DomainEvent<Guid>
	{
		public Guid CandidateID { get; set; }
		public string CandidateName { get; set; }
		public string EmailAddress { get; set; }
	}
}
