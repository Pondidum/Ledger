using System;

namespace Ledger.Acceptance.AcceptanceTests
{
	public class AcceptanceBase<TAggregate>
	{
		public TAggregate Aggregate { get; set; }

		private IEventStore<Guid> _store;

		protected virtual IEventStore<Guid> EventStore
		{
			get
			{
				_store = _store ?? StoreBuilder.GetStore();
				return _store;
			}
		}
	}
}
