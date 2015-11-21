using System;

namespace Ledger.Acceptance.TestDomain.Events
{
	public class NameChangedByDeedPoll : DomainEvent<Guid>
	{
		public string NewName { get; set; }
	}
}
