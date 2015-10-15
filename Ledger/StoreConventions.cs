using System;

namespace Ledger
{
	public class StoreStoreConventions : IStoreConventions
	{
		private readonly IStoreNamingConvention _namingConvention;
		private readonly Type _key;
		private readonly Type _aggregate;

		public StoreStoreConventions(IStoreNamingConvention namingConvention, Type key, Type aggregate)
		{
			_namingConvention = namingConvention;
			_key = key;
			_aggregate = aggregate;
		}

		public string EventStoreName()
		{
			return _namingConvention.ForEvents(_key, _aggregate);
		}

		public string SnapshotStoreName()
		{
			return _namingConvention.ForSnapshots(_key, _aggregate);
		}
	}
}
