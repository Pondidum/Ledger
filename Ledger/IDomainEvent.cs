namespace Ledger
{
	public interface IDomainEvent<TKey> : IStamped
	{
		TKey AggregateID { get; set; }
	}
}
