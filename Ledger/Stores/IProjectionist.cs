namespace Ledger.Stores
{
	public interface IProjectionist
	{
		void Project<TKey>(DomainEvent<TKey> domainEvent);
	}
}
