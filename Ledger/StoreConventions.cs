using System;

namespace Ledger
{
	public class StoreConventions : IStoreConventions
	{
		private readonly IStoreNamingConvention _namingConvention;

		public StoreConventions(IStoreNamingConvention namingConvention, Type key, Type aggregate)
		{
			_namingConvention = namingConvention;
			KeyType = key;
			AggregateType = aggregate;
		}

		public Type KeyType { get; }
		public Type AggregateType { get; }

		public string EventStoreName()
		{
			return _namingConvention.ForEvents(KeyType, AggregateType);
		}

		public string SnapshotStoreName()
		{
			return _namingConvention.ForSnapshots(KeyType, AggregateType);
		}
	}
}
