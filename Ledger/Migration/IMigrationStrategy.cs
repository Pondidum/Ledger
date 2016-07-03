namespace Ledger.Migration
{
	public interface IMigrationStrategy
	{
		void Execute<TKey>(IStoreReader<TKey> reader, IStoreWriter<TKey> writer);
	}
}
