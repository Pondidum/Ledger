using System;
using Ledger.Stores.Memory;
using StructureMap;
using StructureMap.Graph;

namespace Ledger.Acceptance.AcceptanceTests
{
	public class AcceptanceBase<TAggregate>
	{
		public TAggregate Aggregate { get; set; }

		private IEventStore<Guid> _store;
		private readonly static Container Container;

		protected virtual IEventStore<Guid> EventStore
		{
			get
			{
				_store = _store ?? Container.GetInstance<IEventStore<Guid>>();
				return _store;
			}
		}

		static AcceptanceBase()
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
	}
}
