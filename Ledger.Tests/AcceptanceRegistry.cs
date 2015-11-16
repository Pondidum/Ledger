using System;
using Ledger.Stores;
using StructureMap.Configuration.DSL;

namespace Ledger.Tests
{
	public class AcceptanceRegistry : Registry
	{
		public AcceptanceRegistry()
		{
			For<IEventStore>().Use<InMemoryEventStore>();
		}
	}
}
