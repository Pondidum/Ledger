using System;
using Dapper;
using Npgsql;
using Shouldly;
using Xunit;

namespace Ledger.Stores.Postgres.Tests
{
	public class CreateGuidKeyedTablesCommandTests : PostgresTestBase, IDisposable
	{
		public CreateGuidKeyedTablesCommandTests()
		{
			CleanupTables();
		}

		private void CleanupTables()
		{
			using (var connection = new NpgsqlConnection(ConnectionString))
			{
				connection.Open();
				connection.Execute("drop table if exists events_guid");
				connection.Execute("drop table if exists snapshots_guid");
			}
		}

		[Fact]
		public void The_tables_should_be_created()
		{
			var command = new CreateGuidKeyedTablesCommand(ConnectionString);
			command.Execute();

			using (var connection = new NpgsqlConnection(ConnectionString))
			{
				connection.Open();

				connection
					.ExecuteScalar<bool>("select exists(select * from information_schema.tables where table_name = 'events_guid')")
					.ShouldBe(true);

				connection
					.ExecuteScalar<bool>("select exists(select * from information_schema.tables where table_name = 'snapshots_guid')")
					.ShouldBe(true);
			}
		}

		public void Dispose()
		{
			CleanupTables();
		}
	}
}
