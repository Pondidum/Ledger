namespace Ledger.Acceptance.TestDomain.Events
{
	public class NameChangedByDeedPoll : DomainEvent
	{
		public string NewName { get; set; }
	}
}
