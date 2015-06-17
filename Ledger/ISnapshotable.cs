namespace Ledger
{
	public interface ISnapshotable<TSnapshot>
		where TSnapshot : ISequenced
	{
		TSnapshot CreateSnapshot();
		void ApplySnapshot(TSnapshot snapshot);
	}
}
