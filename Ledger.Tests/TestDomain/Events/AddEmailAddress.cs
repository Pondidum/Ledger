namespace Ledger.Tests.TestDomain.Events
{
	public class AddEmailAddress : DomainEvent
	{
		public string Email { get; set; }
	}
}
