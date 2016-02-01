using System;

namespace Ledger
{
	public interface IDomainEvent<TKey> 
	{
		TKey AggregateID { get; set; }
		DateTime Stamp { get; set; }
	}
}
