using System;

namespace Ledger
{
	public class DomainEvent<TKey> : IDomainEvent<TKey>
	{
		public DateTime Sequence { get; set; }
		public TKey AggregateID { get; set; }
	}
}
