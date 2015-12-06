using System;

namespace Ledger.Tests.AggregateStoreTests.MiniDomain.Events
{
	public class RoleNameChangedEvent : DomainEvent<Guid>
	{
		public string NewName { get; set; }
	}
}
