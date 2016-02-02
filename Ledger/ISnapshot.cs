using System;

namespace Ledger
{
	public interface ISnapshot<TKey>
	{
		TKey AggregateID { get; set; }
		int Sequence { get; set; }
		DateTime Stamp { get; set; }
	}
}
