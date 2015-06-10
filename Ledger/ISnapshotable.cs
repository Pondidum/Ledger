namespace Ledger
{
	public interface ISnapshotable<TSnapshot> where TSnapshot : ISnapshot
	{
		TSnapshot CreateSnapshot();
		void ApplySnapshot(TSnapshot snapshot);
	}
}
