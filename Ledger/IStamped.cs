using System;

namespace Ledger
{
	public interface IStamped
	{
		DateTime Stamp { get; set; }
	}
}
