using System;
using System.Runtime.Serialization;

namespace Ledger
{
	public class ConsistencyException : Exception
	{
		public ConsistencyException()
		{
		}

		public ConsistencyException(Type aggregate, string id, DateTime sequenceID, DateTime? lastStoredSequence)
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
