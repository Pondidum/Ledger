using Ledger.Infrastructure;

namespace Ledger.Projections
{
	public class Projection : IProjectionist
	{
		public void Apply<TKey>(DomainEvent<TKey> domainEvent)
		{
			this.Handle(domainEvent, throwOnMissing: false);
		}
	}
}
