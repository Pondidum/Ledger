namespace Ledger
{
	public interface IEventStore
	{
		IStoreReader<TKey> CreateReader<TKey>(IStoreConventions storeConventions);
		IStoreWriter<TKey> CreateWriter<TKey>(IStoreConventions storeConventions);
	}
}
