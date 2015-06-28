using Npgsql;
using Xunit;

namespace Ledger.Stores.Postgres.Tests
{
	public class CreateGuidKeyedTablesCommandTests : PostgresTestBase
	{
		public CreateGuidKeyedTablesCommandTests()
		{
			using (var connection = new NpgsqlConnection(ConnectionString))
			{
				connection.Open();
				Checkpoint.Reset(connection);
			}
		}

		[Fact]
		public void The_tables_should_be_created()
		{
			var command = new CreateGuidKeyedTablesCommand(ConnectionString);
			command.Execute();
		}
	}
}
