namespace Ledger
{
	public interface ISnapshotable<TKey, TSnapshot>
		where TSnapshot : ISnapshot<TKey>
	{
		TSnapshot CreateSnapshot();
		void ApplySnapshot(TSnapshot snapshot);
	}
}
