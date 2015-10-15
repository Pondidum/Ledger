using System;

namespace Ledger
{
	public interface IStoreConventions
	{
		Type KeyType { get; }
		Type AggregateType { get; }

		string EventStoreName();
		string SnapshotStoreName();
	}
}
