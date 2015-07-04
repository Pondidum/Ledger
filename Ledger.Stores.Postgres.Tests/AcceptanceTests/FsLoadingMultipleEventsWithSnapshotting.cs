using System;
using System.IO;
using Ledger.Tests.AcceptanceTests;

namespace Ledger.Stores.Postgres.Tests.AcceptanceTests
{
	public class PostgresLoadingMultipleEventsWithSnapshotting : LoadingMultipleEventsWithSnapshotting
	{
		private PostgresEventStore<Guid> _store;

		protected override IEventStore<Guid> EventStore
		{
			get
			{
				_store = _store ?? new PostgresEventStore<Guid>(PostgresTestBase.ConnectionString);
				return _store;
			}
		}
	}
}