namespace Ledger.Projections
{
	public interface IProjectionist
	{
		void Apply<TKey>(DomainEvent<TKey> domainEvent);
	}
}
