using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Newtonsoft.Json;
using Npgsql;

namespace Ledger.Stores.Postgres
{
	public class PostgresEventStore<TKey> : IEventStore<TKey>
	{
		private readonly ITableName _tableName;
		private readonly NpgsqlConnection _connection;
		private readonly JsonSerializerSettings _jsonSettings;
		private readonly NpgsqlTransaction _transaction;

		public PostgresEventStore(NpgsqlConnection connection)
			: this(connection, new KeyTypeTableName())
		{
		}

		public PostgresEventStore(NpgsqlConnection connection, ITableName tableName)
			: this(connection, null, tableName)
		{
		}

		private PostgresEventStore(NpgsqlConnection connection, NpgsqlTransaction transaction, ITableName tableName)
		{
			_connection = connection;
			_transaction = transaction;
			_tableName = tableName;
			_jsonSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
		}

		public void CreateTable()
		{
			var builder = new TableBuilder(_connection, _tableName);
			builder.CreateTable<TKey>();
		}

		private string Events(string sql)
		{
			return sql.Replace("{table}", _tableName.ForEvents<TKey>());
		}

		private string Snapshots(string sql)
		{
			return sql.Replace("{table}", _tableName.ForSnapshots<TKey>());
		}

		public int? GetLatestSequenceFor(TKey aggregateID)
		{
			var sql = Events("select max(sequence) from {table} where aggregateID = @id");

			return _connection.ExecuteScalar<int>(sql, new { ID = aggregateID });
		}

		public int? GetLatestSnapshotSequenceFor(TKey aggregateID)
		{
			var sql = Snapshots("select max(sequence) from {table} where aggregateID = @id");

			return _connection.ExecuteScalar<int>(sql, new { ID = aggregateID });
		}

		public void SaveEvents(TKey aggregateID, IEnumerable<DomainEvent> changes)
		{
			var sql = Events("insert into {table} (aggregateID, sequence, event) values (@id, @sequence, @event::json);");

			foreach (var change in changes)
			{
				_connection.Execute(sql, new
				{
					ID = aggregateID,
					Sequence = change.Sequence,
					Event = JsonConvert.SerializeObject(change, _jsonSettings)
				});
			}
		}

		public IEnumerable<DomainEvent> LoadEvents(TKey aggregateID)
		{
			var sql = Events("select event from {table} where aggregateID = @id order by sequence asc");

			return _connection
				.Query<string>(sql, new { ID = aggregateID })
				.Select(json => JsonConvert.DeserializeObject<DomainEvent>(json, _jsonSettings))
				.ToList();
		}

		public IEnumerable<DomainEvent> LoadEventsSince(TKey aggregateID, int sequenceID)
		{
			var sql = Events("select event from {table} where aggregateID = @id and sequence > @last order by sequence asc");

			return _connection
				.Query<string>(sql, new { ID = aggregateID, Last = sequenceID })
				.Select(json => JsonConvert.DeserializeObject<DomainEvent>(json, _jsonSettings))
				.ToList();
		}

		public ISequenced LoadLatestSnapshotFor(TKey aggregateID)
		{
			var sql = Snapshots("select snapshot from {table} where aggregateID = @id order by sequence desc limit 1");

			return _connection
				.Query<string>(sql, new { ID = aggregateID })
				.Select(json => JsonConvert.DeserializeObject<ISequenced>(json, _jsonSettings))
				.FirstOrDefault();
		}

		public void SaveSnapshot(TKey aggregateID, ISequenced snapshot)
		{
			var sql = Snapshots("insert into {table} (aggregateID, sequence, snapshot) values (@id, @sequence, @snapshot::json);");

			_connection.Execute(sql, new
			{
				ID = aggregateID,
				Sequence = snapshot.Sequence,
				Snapshot = JsonConvert.SerializeObject(snapshot, _jsonSettings)
			});
		}

		public IEventStore<TKey> BeginTransaction()
		{
			if (_connection.State != ConnectionState.Open)
			{
				_connection.Open();
			}

			return new PostgresEventStore<TKey>(_connection, _connection.BeginTransaction(), _tableName);
		}

		public void Dispose()
		{
			if (_transaction != null)
			{
				_transaction.Commit();
			}

			_connection.Close();
		}
	}
}
