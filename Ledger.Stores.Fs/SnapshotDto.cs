namespace Ledger.Stores.Fs
{
	public class SnapshotDto<TKey>
	{
		public TKey ID { get; set; }
		public object Snapshot { get; set; }
	}
}
