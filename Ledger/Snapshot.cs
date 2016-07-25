using System;
using Ledger.Infrastructure;

namespace Ledger
{
	public abstract class Snapshot<TKey>
	{
		public abstract TKey AggregateID { get; set; }
		public abstract Sequence Sequence { get; set; }
		public abstract DateTime Stamp { get; set; }
	}
}
