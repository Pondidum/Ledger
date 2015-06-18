using System;
using Ledger;

namespace TestsDomain.Events
{
	public class FixNameSpelling : DomainEvent
	{
		public string NewName { get; set; }
	}
}
