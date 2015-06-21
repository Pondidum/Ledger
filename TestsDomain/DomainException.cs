using System;

namespace TestsDomain
{
	public class DomainException : Exception
	{
		public DomainException(string message)
			: base(message)
		{
		}
	}
}
