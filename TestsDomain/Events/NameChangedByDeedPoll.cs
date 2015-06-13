using System;
using Ledger;

namespace TestsDomain.Events
{
	public class NameChangedByDeedPoll : DomainEvent<Guid>
	{
		public string NewName { get; set; }
	}
}
