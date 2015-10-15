using System;

namespace Ledger.Conventions
{
	public class AggregateTypeNamingConvention : IStoreNamingConvention
	{
		public string ForEvents(Type key, Type aggregate)
		{
			return aggregate.Name.ToLower() + "_events";
		}

		public string ForSnapshots(Type key, Type aggregate)
		{
			return aggregate.Name.ToLower() + "_snapshots";
		}
	}
}
