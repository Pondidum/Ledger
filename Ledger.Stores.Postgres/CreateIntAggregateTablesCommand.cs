using Dapper;
using Npgsql;

namespace Ledger.Stores.Postgres
{
	public class CreateIntAggregateTablesCommand
	{
		private const string Sql = @"
create table if not exists {events-table} (
	id serial primary key,
	aggregateID int not null,
	sequence integer not null,
	event json not null
);

create table if not exists {snapshots-table} (
	id serial primary key,
	aggregateID int not null,
	sequence integer not null,
	snapshot json not null
);
";

		private readonly NpgsqlConnection _connection;
		private readonly ITableName _tableName;

		public CreateIntAggregateTablesCommand(NpgsqlConnection connection)
			: this(connection, new KeyTypeTableName())
		{
		}

		public CreateIntAggregateTablesCommand(NpgsqlConnection connection, ITableName tableName)
		{
			_connection = connection;
			_tableName = tableName;
		}

		public void Execute()
		{
			var sql = Sql
				.Replace("{events-table}", _tableName.ForEvents<int>())
				.Replace("{snapshots-table}", _tableName.ForSnapshots<int>());

			_connection.Execute(sql);

		}
	}
}
