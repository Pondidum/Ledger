using Ledger;

namespace TestsDomain.Events
{
	public class AddEmailAddress : DomainEvent
	{
		public string Email { get; set; }
	}
}
