using System;
using Ledger.Stores.Memory;

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
				_store = _store ?? new InMemoryEventStore<Guid>();
				return _store;
			}
		}
	}
}
