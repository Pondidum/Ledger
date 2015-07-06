using System;
using System.Data;
using Dapper;
using Npgsql;

namespace Ledger.Stores.Postgres.Tests
{
	public class PostgresTestBase : IDisposable
	{
		public const string ConnectionString = "PORT=5432;TIMEOUT=15;POOLING=True;MINPOOLSIZE=1;MAXPOOLSIZE=20;COMMANDTIMEOUT=20;COMPATIBLE=2.1.3.0;HOST=192.168.59.103;USER ID=postgres;DATABASE=postgres";
		public NpgsqlConnection Connection { get; set; }

		public PostgresTestBase()
		{
			Connection = new NpgsqlConnection(ConnectionString);

			Connection.Open();
			
			var create = new CreateGuidAggregateTablesCommand(Connection);
			create.Execute();
		}

		public void Dispose()
		{
			if (Connection.State != ConnectionState.Closed)
			{
				Connection.Close();
			}
		}
	}
}
