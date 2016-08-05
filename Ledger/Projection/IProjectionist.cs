namespace Ledger.Projection
{
	public interface IProjectionist
	{
		void Project<TKey>(DomainEvent<TKey> domainEvent);
	}
}
