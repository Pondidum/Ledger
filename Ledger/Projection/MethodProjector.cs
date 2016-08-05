using Ledger.Stores;

namespace Ledger.Projection
{
	public class MethodProjector : IProjectionist
	{
		public void Project<TKey>(DomainEvent<TKey> domainEvent)
		{
			throw new System.NotImplementedException();
		}
	}
}
