using System;
using Npgsql;
using StructureMap.Configuration.DSL;

namespace Ledger.Stores.Postgres.Tests
{
	public class AcceptanceRegistry : Registry
	{
		public AcceptanceRegistry()
		{
			For<IEventStore<Guid>>().Use(() => new PostgresEventStore<Guid>(new NpgsqlConnection(PostgresTestBase.ConnectionString)));
		}
	}
}
