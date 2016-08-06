namespace Ledger.Projection
{
	public interface IProjectionist
	{
		void Apply<TKey>(DomainEvent<TKey> domainEvent);
	}
}
