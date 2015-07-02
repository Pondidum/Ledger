using System;
using Dapper;
using Npgsql;

namespace Ledger.Stores.Postgres.Tests
{
	public class PostgresTestBase
	{
		public static string ConnectionString { get; set; }

		static PostgresTestBase()
		{
			ConnectionString = "PORT=5432;TIMEOUT=15;POOLING=True;MINPOOLSIZE=1;MAXPOOLSIZE=20;COMMANDTIMEOUT=20;COMPATIBLE=2.1.3.0;HOST=192.168.59.103;USER ID=postgres;DATABASE=postgres";

			using (var connection = new NpgsqlConnection(ConnectionString))
			{
				connection.Open();
				connection.Execute("drop table if exists events_guid");
				connection.Execute("drop table if exists snapshots_guid");
			}

			var create = new CreateGuidKeyedTablesCommand(ConnectionString);
			create.Execute();
		}
	}
}
