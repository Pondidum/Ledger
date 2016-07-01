namespace Ledger
{
	public interface IMigrationStrategy
	{
		void Execute<TKey>(IStoreReader<TKey> reader, IStoreWriter<TKey> writer);
	}
}
