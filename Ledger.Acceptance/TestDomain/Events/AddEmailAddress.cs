namespace Ledger.Acceptance.TestDomain.Events
{
	public class AddEmailAddress : DomainEvent
	{
		public string Email { get; set; }
	}
}
