using System;
using System.Runtime.Serialization;
using Ledger.Infrastructure;

namespace Ledger
{
	public class ConsistencyException : Exception
	{
		public ConsistencyException()
		{
		}

		public ConsistencyException(Type aggregate, string id, Sequence sequenceID, Sequence? lastStoredSequence)
			: base($"{aggregate.Name} {id} base sequence is {sequenceID}, but the store's is {lastStoredSequence}, so it cannot be saved.")
		{
		}

		public ConsistencyException(string message) : base(message)
		{
		}

		public ConsistencyException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected ConsistencyException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
