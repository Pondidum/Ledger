using System;

namespace Ledger.Acceptance.TestDomain.Events
{
	public class FixNameSpelling : DomainEvent<Guid>
	{
		public string NewName { get; set; }
	}
}
