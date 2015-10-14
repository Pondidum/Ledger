using System;

namespace Ledger
{
	public interface IStoreNamingConvention
	{
		string ForEvents(Type key, Type aggregate);
		string ForSnapshots(Type key, Type aggregate);
	}
}
