using System;

namespace Ledger.Tests.TestDomain
{
	public class DomainException : Exception
	{
		public DomainException(string message)
			: base(message)
		{
		}
	}
}
