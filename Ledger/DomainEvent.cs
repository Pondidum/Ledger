using System;

namespace Ledger
{
	public class DomainEvent<TKey>
	{
		public DateTime Stamp { get; set; }
		public TKey AggregateID { get; set; }
		public int Sequence { get; set; }
	}
}
