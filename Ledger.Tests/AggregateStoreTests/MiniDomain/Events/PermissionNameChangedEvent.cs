using System;

namespace Ledger.Tests.AggregateStoreTests.MiniDomain.Events
{
	public class PermissionNameChangedEvent : DomainEvent<Guid>
	{
		public string NewName { get; set; }
	}
}
