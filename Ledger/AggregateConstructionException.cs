using System;
using System.Runtime.Serialization;

namespace Ledger
{
	public class AggregateConstructionException : Exception
	{
		public AggregateConstructionException()
		{
		}

		public AggregateConstructionException(string message) : base(message)
		{
		}

		public AggregateConstructionException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected AggregateConstructionException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
