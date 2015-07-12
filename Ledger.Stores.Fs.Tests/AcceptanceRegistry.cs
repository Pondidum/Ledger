using System;
using StructureMap.Configuration.DSL;

namespace Ledger.Stores.Fs.Tests
{
	public class AcceptanceRegistry : Registry
	{
		public AcceptanceRegistry()
		{
			For<IEventStore<Guid>>().Use<WrappedStore<Guid>>();
		}
	}

	public class WrappedStore<TKey> : FileEventStore<TKey>
	{
		public WrappedStore()
			: base(Guid.NewGuid().ToString())
		{
			System.IO.Directory.CreateDirectory(Directory);
		}
	}
}
