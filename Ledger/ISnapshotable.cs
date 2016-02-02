namespace Ledger
{
	public interface ISnapshotable<TKey, TSnapshot>
		where TSnapshot : Snapshot<TKey>
	{
		TSnapshot CreateSnapshot();
		void ApplySnapshot(TSnapshot snapshot);
	}
}
