namespace Ledger.Acceptance.TestDomain.Events
{
	public class FixNameSpelling : DomainEvent
	{
		public string NewName { get; set; }
	}
}
