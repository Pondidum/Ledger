namespace Ledger.Stores.Postgres
{
	public interface ITableName
	{
		string ForEvents<TKey>();
		string ForSnapshots<TKey>();
	}
}
