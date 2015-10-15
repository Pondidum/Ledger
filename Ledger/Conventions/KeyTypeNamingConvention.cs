using System;

namespace Ledger.Conventions
{
	public class KeyTypeNamingConvention : IStoreNamingConvention
	{
		public string ForEvents(Type key, Type aggregate)
		{
			return "events_" + key.Name.ToLower();
		}

		public string ForSnapshots(Type key, Type aggregate)
		{
			return "snapshots_" + key.Name.ToLower();
		}
	}
}
