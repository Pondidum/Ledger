namespace Ledger
{
	public class DomainEvent<TKey>
	{
		public int SequenceID { get; set; }
		public TKey AggregateID { get; set; }
	}
}
