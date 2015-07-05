using Dapper;
using Npgsql;

namespace Ledger.Stores.Postgres
{
	public class CreateGuidKeyedTablesCommand
	{
		private const string Sql = @"
create extension if not exists ""uuid-ossp"";

create table if not exists events_guid (
	id uuid primary key default uuid_generate_v4(),
	aggregateID uuid not null,
	sequence integer not null,
	event json not null
);

create table if not exists snapshots_guid (
	id uuid primary key default uuid_generate_v4(),
	aggregateID uuid not null,
	sequence integer not null,
	snapshot json not null
);
";
		private readonly NpgsqlConnection _connection;

		public CreateGuidKeyedTablesCommand(NpgsqlConnection connection)
		{
			_connection = connection;
		}

		public void Execute()
		{
			_connection.Execute(Sql);
		}
	}
}
