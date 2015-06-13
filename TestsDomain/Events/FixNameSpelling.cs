using System;
using Ledger;

namespace TestsDomain.Events
{
	public class FixNameSpelling : DomainEvent<Guid>
	{
		public string NewName { get; set; }
	}
}
