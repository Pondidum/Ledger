namespace Ledger.Stores
{
	public class InterceptingMaintainer<TKey> : IStoreMaintainer<TKey>
	{
		private readonly IStoreMaintainer<TKey> _other;

		public InterceptingMaintainer(IStoreMaintainer<TKey> other)
		{
			_other = other;
		}

		public void RemoveAllOldSnapshots(TKey aggregateID)
		{
			_other.RemoveAllOldSnapshots(aggregateID);
		}

		public void KeepLastSnapshots(TKey aggregateID, int snapshotsToKeep)
		{
			_other.KeepLastSnapshots(aggregateID, snapshotsToKeep);
		}

		public void Dispose()
		{
			_other.Dispose();
		}
	}
}
