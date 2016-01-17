namespace Ledger
{
	public interface IEventStore
	{
		IStoreReader<TKey> CreateReader<TKey>(string streamName);
		IStoreWriter<TKey> CreateWriter<TKey>(string streamName);
		IStoreMaintainer<TKey> CreateMaintainer<TKey>(string streamName);
	}
}
