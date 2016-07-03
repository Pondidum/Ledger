namespace Ledger.Migration
{
	public class BlankDestinationStrategy : IMigrationStrategy
	{
		public void Execute<TKey>(IStoreReader<TKey> reader, IStoreWriter<TKey> writer)
		{
			var keys = reader.LoadAllKeys();

			foreach (var key in keys)
			{
				writer.SaveEvents(reader.LoadEvents(key));

				var snapshot = reader.LoadLatestSnapshotFor(key);

				if (snapshot != null)
					writer.SaveSnapshot(snapshot);
			}
		}
	}
}
