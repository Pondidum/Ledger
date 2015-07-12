using System;
using StructureMap.Configuration.DSL;

namespace Ledger.Stores.Memory.Tests
{
	public class AcceptanceRegistry : Registry
	{
		public AcceptanceRegistry()
		{
			For<IEventStore<Guid>>().Use<InMemoryEventStore<Guid>>();
		}
	}
}
