using System;

namespace Ledger.Acceptance.TestDomain
{
	public class DomainException : Exception
	{
		public DomainException(string message)
			: base(message)
		{
		}
	}
}
