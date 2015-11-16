using System;
using StructureMap;
using StructureMap.Graph;

namespace Ledger.Acceptance.AcceptanceTests
{
	public class StoreBuilder
	{
		private readonly static Container Container;

		static StoreBuilder()
		{
			Container = new Container(config =>
			{
				config.Scan(a =>
				{
					a.AssembliesFromApplicationBaseDirectory();
					a.LookForRegistries();
				});
			});
		}

		public static IEventStore GetStore()
		{
			return Container.GetInstance<IEventStore>();
		}
	}
}
