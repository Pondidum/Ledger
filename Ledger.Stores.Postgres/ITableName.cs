namespace Ledger.Stores.Postgres
{
	public interface ITableName
	{
		string ForEvents<TKey>();
		string ForSnapshots<TKey>();
	}

	public class KeyTypeTableName : ITableName
	{
		public string ForEvents<TKey>()
		{
			return "events_" + typeof(TKey).Name.ToLower();
		}

		public string ForSnapshots<TKey>()
		{
			return "snapshots_" + typeof(TKey).Name.ToLower();
		}
	}
}