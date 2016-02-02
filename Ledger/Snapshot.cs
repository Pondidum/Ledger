using System;

namespace Ledger
{
	public abstract class Snapshot<TKey>
	{
		public abstract TKey AggregateID { get; set; }
		public abstract int Sequence { get; set; }
		public abstract DateTime Stamp { get; set; }
	}
}
