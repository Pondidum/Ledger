namespace Ledger
{
	public class DomainEvent : ISequenced
	{
		public int SequenceID { get; set; }
	}
}
