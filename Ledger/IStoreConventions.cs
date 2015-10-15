namespace Ledger
{
	public interface IStoreConventions
	{
		string EventStoreName();
		string SnapshotStoreName();
	}
}
