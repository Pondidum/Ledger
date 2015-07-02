namespace Ledger
{
	public class DomainEvent : ISequenced
	{
		public int Sequence { get; set; }
	}
}
