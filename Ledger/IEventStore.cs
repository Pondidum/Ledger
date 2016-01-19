namespace Ledger
{
	public interface IEventStore
	{
		IStoreReader<TKey> CreateReader<TKey>(EventStoreContext context);
		IStoreWriter<TKey> CreateWriter<TKey>(EventStoreContext context);
		IStoreMaintainer<TKey> CreateMaintainer<TKey>(EventStoreContext context);
	}
}
