using System;

namespace Ledger.Tests.AggregateStoreTests.MiniDomain.Events
{
	public class UserCreatedEvent : DomainEvent<Guid> { }
}