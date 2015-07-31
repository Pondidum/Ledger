using System;

namespace Ledger
{
	public class ConsistencyException : Exception
	{
		public ConsistencyException(Type aggregate, string id, int sequenceID, int? lastStoredSequence)
			: base(string.Format(
			"{0} {1} base sequence is {2}, but the store's is {3}, so it cannot be saved.", aggregate.Name, id, sequenceID, lastStoredSequence.ToString()))
		{
		}
	}
}
