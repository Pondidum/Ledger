using System;
using System.IO;
using Ledger.Tests.AcceptanceTests;
using Npgsql;

namespace Ledger.Stores.Postgres.Tests.AcceptanceTests
{
	public class PostgresLoadingMultipleEventsWithSnapshotting : LoadingMultipleEventsWithSnapshotting
	{
		private PostgresEventStore<Guid> _store;

		protected override IEventStore<Guid> EventStore
		{
			get
			{
				_store = _store ?? new PostgresEventStore<Guid>(new NpgsqlConnection(PostgresTestBase.ConnectionString));
				return _store;
			}
		}
	}
}