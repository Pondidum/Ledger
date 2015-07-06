using Dapper;
using Npgsql;

namespace Ledger.Stores.Postgres
{
	public class CreateIntAggregateTablesCommand
	{
		private const string Sql = @"
create table if not exists events_int32 (
	id serial primary key,
	aggregateID int not null,
	sequence integer not null,
	event json not null
);

create table if not exists snapshots_int32 (
	id serial primary key,
	aggregateID int not null,
	sequence integer not null,
	snapshot json not null
);
";

		private readonly NpgsqlConnection _connection;

		public CreateIntAggregateTablesCommand(NpgsqlConnection connection)
		{
			_connection = connection;
		}

		public void Execute()
		{
			_connection.Execute(Sql);
		}
	}
}
