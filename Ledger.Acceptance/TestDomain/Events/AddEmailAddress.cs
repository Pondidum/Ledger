using System;

namespace Ledger.Acceptance.TestDomain.Events
{
	public class AddEmailAddress : DomainEvent<Guid>
	{
		public string Email { get; set; }
	}
}
