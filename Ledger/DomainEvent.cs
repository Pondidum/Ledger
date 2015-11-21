namespace Ledger
{
	public class DomainEvent<TKey> : IDomainEvent<TKey>
	{
		public int Sequence { get; set; }
		public TKey AggregateID { get; set; }
	}
}
