namespace Ledger
{
	public interface ISnapshot<TKey> : IStamped
	{
		TKey AggregateID { get; set; }
	}
}
