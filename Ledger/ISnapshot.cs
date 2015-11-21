namespace Ledger
{
	public interface ISnapshot<TKey> : ISequenced
	{
		TKey AggregateID { get; set; }
	}
}
