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
		private readonly string _connectionString;
		private readonly JsonSerializerSettings _jsonSettings;

		public PostgresEventStore(string connectionString)
		{
			_connectionString = connectionString;
			_jsonSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
		}

		private IDbConnection Open()
		{
			var connection = new NpgsqlConnection(_connectionString);
			connection.Open();

			return connection;
		}

		public int? GetLatestSequenceFor(TKey aggregateID)
		{
			var sql = "select max(sequence) from events_guid where aggregateID = @id";

			using (var connection = Open())
			{
				return connection.ExecuteScalar<int>(sql, new { ID = aggregateID });
			}
		}

		public int? GetLatestSnapshotSequenceFor(TKey aggregateID)
		{
			var sql = "select max(sequence) from snapshots_guid where aggregateID = @id";

			using (var connection = Open())
			{
				return connection.ExecuteScalar<int>(sql, new { ID = aggregateID });
			}
		}

		public void SaveEvents(TKey aggregateID, IEnumerable<DomainEvent> changes)
		{
			var sql = "insert into events_guid (aggregateID, sequence, event) values (@id, @sequence, @event::json);";

			using (var connection = Open())
			{
				foreach (var change in changes)
				{
					connection.Execute(sql, new
					{
						ID = aggregateID,
						Sequence = change.Sequence,
 						Event = JsonConvert.SerializeObject(change, _jsonSettings)
					});
				}
			}
		}

		public IEnumerable<DomainEvent> LoadEvents(TKey aggregateID)
		{
			var sql = "select event from events_guid where aggregateID = @id order by sequence asc";

			using (var connection = Open())
			{
				return connection
					.Query<string>(sql, new { ID = aggregateID})
					.Select(json => JsonConvert.DeserializeObject<DomainEvent>(json, _jsonSettings));
			}
		}

		public IEnumerable<DomainEvent> LoadEventsSince(TKey aggregateID, int sequenceID)
		{
			var sql = "select event from events_guid where aggregateID = @id and sequence > @last order by sequence asc";

			using (var connection = Open())
			{
				return connection
					.Query<string>(sql, new { ID = aggregateID, Last = sequenceID })
					.Select(json => JsonConvert.DeserializeObject<DomainEvent>(json, _jsonSettings));
			}
		}

		public ISequenced LoadLatestSnapshotFor(TKey aggregateID)
		{
			var sql = "select snapshot from snapshots_guid where aggregateID = @id order by sequence desc limit 1";

			using (var connection = Open())
			{
				return connection
					.Query<string>(sql, new {ID = aggregateID})
					.Select(json => JsonConvert.DeserializeObject<ISequenced>(json, _jsonSettings))
					.FirstOrDefault();
			}
		}

		public void SaveSnapshot(TKey aggregateID, ISequenced snapshot)
		{
			var sql = "insert into snapshots_guid (aggregateID, sequence, snapshot) values (@id, @sequence, @snapshot::json);";

			using (var connection = Open())
			{
				connection.Execute(sql, new
				{
					ID = aggregateID,
					Sequence = snapshot.Sequence,
					Snapshot = JsonConvert.SerializeObject(snapshot, _jsonSettings)
				});
			}
		}

		public void Dispose()
		{
		}
	}
}
