using System;

namespace Ledger
{
	public interface ISequenced
	{
		DateTime Sequence { get; set; }
	}
}
