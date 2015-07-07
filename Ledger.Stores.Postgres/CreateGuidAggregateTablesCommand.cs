using System;
using Dapper;
using Npgsql;

namespace Ledger.Stores.Postgres
{
	public class CreateGuidAggregateTablesCommand
	{
		private const string Sql = @"
create extension if not exists ""uuid-ossp"";

create table if not exists {events-table} (
	id uuid primary key default uuid_generate_v4(),
	aggregateID uuid not null,
	sequence integer not null,
	event json not null
);

create table if not exists {snapshots-table} (
	id uuid primary key default uuid_generate_v4(),
	aggregateID uuid not null,
	sequence integer not null,
	snapshot json not null
);
";
		private readonly NpgsqlConnection _connection;
		private readonly ITableName _tableName;

		public CreateGuidAggregateTablesCommand(NpgsqlConnection connection)
			:this(connection, new KeyTypeTableName())
		{
		}

		public CreateGuidAggregateTablesCommand(NpgsqlConnection connection, ITableName tableName)
		{
			_connection = connection;
			_tableName = tableName;
		}

		public void Execute()
		{
			var sql = Sql
				.Replace("{events-table}", _tableName.ForEvents<Guid>())
				.Replace("{snapshots-table}", _tableName.ForSnapshots<Guid>());

			_connection.Execute(sql);
		}
	}
}
