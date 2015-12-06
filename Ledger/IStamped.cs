using System;

namespace Ledger
{
	public interface IStamped
	{
		DateTime Sequence { get; set; }
	}
}
