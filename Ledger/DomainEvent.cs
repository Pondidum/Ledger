using System;
using Ledger.Infrastructure;

namespace Ledger
{
	public class DomainEvent<TKey>
	{
		public DateTime Stamp { get; set; }
		public TKey AggregateID { get; set; }
		public Sequence Sequence { get; set; }
		public GlobalSequence GlobalSequence { get; set; }
	}
}
