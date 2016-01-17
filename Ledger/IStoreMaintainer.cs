using System;

namespace Ledger
{
	public interface IStoreMaintainer<TKey> : IDisposable
	{
		void RemoveAllOldSnapshots(TKey aggregateID);
		void KeepLastSnapshots(TKey aggregateID, int snapshotsToKeep);
	}
}
