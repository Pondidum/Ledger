using Ledger.Infrastructure;

namespace Ledger.Projection
{
	public class Projection : IProjectionist
	{
		public void Project<TKey>(DomainEvent<TKey> domainEvent)
		{
			this.AsDynamic().Handle(domainEvent);
		}
	}
}
