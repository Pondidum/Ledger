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
		private readonly NpgsqlConnection _connection;
		private readonly JsonSerializerSettings _jsonSettings;
		private readonly NpgsqlTransaction _transaction;

		public PostgresEventStore(NpgsqlConnection connection)
			: this(connection, null)
		{
		}

		public PostgresEventStore(NpgsqlConnection connection, NpgsqlTransaction transaction)
		{
			_connection = connection;
			_transaction = transaction;
			_jsonSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
		}

		public int? GetLatestSequenceFor(TKey aggregateID)
		{
			var sql = "select max(sequence) from events_guid where aggregateID = @id";

			return _connection.ExecuteScalar<int>(sql, new { ID = aggregateID });
		}

		public int? GetLatestSnapshotSequenceFor(TKey aggregateID)
		{
			var sql = "select max(sequence) from snapshots_guid where aggregateID = @id";

			return _connection.ExecuteScalar<int>(sql, new { ID = aggregateID });
		}

		public void SaveEvents(TKey aggregateID, IEnumerable<DomainEvent> changes)
		{
			var sql = "insert into events_guid (aggregateID, sequence, event) values (@id, @sequence, @event::json);";

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
			var sql = "select event from events_guid where aggregateID = @id order by sequence asc";

			return _connection
				.Query<string>(sql, new { ID = aggregateID })
				.Select(json => JsonConvert.DeserializeObject<DomainEvent>(json, _jsonSettings))
				.ToList();
		}

		public IEnumerable<DomainEvent> LoadEventsSince(TKey aggregateID, int sequenceID)
		{
			var sql = "select event from events_guid where aggregateID = @id and sequence > @last order by sequence asc";

			return _connection
				.Query<string>(sql, new { ID = aggregateID, Last = sequenceID })
				.Select(json => JsonConvert.DeserializeObject<DomainEvent>(json, _jsonSettings))
				.ToList();
		}

		public ISequenced LoadLatestSnapshotFor(TKey aggregateID)
		{
			var sql = "select snapshot from snapshots_guid where aggregateID = @id order by sequence desc limit 1";

			return _connection
				.Query<string>(sql, new { ID = aggregateID })
				.Select(json => JsonConvert.DeserializeObject<ISequenced>(json, _jsonSettings))
				.FirstOrDefault();
		}

		public void SaveSnapshot(TKey aggregateID, ISequenced snapshot)
		{
			var sql = "insert into snapshots_guid (aggregateID, sequence, snapshot) values (@id, @sequence, @snapshot::json);";

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

			return new PostgresEventStore<TKey>(_connection, _connection.BeginTransaction());
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
