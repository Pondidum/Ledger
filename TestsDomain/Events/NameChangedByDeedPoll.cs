using System;
using Ledger;

namespace TestsDomain.Events
{
	public class NameChangedByDeedPoll : DomainEvent
	{
		public string NewName { get; set; }
	}
}
