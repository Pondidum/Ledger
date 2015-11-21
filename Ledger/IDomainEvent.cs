namespace Ledger
{
	public interface IDomainEvent<TKey> : ISequenced
	{
		TKey AggregateID { get; set; }
	}

	public interface ISnapshot : ISequenced
	{
	}
}
