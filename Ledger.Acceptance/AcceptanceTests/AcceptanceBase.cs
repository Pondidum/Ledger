using System;

namespace Ledger.Acceptance.AcceptanceTests
{
	public class AcceptanceBase<TAggregate>
	{
		public TAggregate Aggregate { get; set; }

		private IEventStore _store;

		protected virtual IEventStore EventStore
		{
			get
			{
				_store = _store ?? StoreBuilder.GetStore();
				return _store;
			}
		}
	}
}
