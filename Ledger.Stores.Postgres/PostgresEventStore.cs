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

		public int? GetLatestSequenceFor(TKey aggegateID)
		{
			var sql = "select max(sequence) from events_guid where aggregateID = @id";

			using (var connection = Open())
			{
				return connection.ExecuteScalar<int>(sql, new {ID = aggegateID});
			}
		}

		public int? GetLatestSnapshotSequenceFor(TKey aggregateID)
		{
			throw new System.NotImplementedException();
		}

		public void SaveEvents(TKey aggegateID, IEnumerable<DomainEvent> changes)
		{
			var sql = "insert into events_guid (aggregateID, sequence, event) values (@id, @sequence, @event::json);";

			using (var connection = Open())
			{
				foreach (var change in changes)
				{
					connection.Execute(sql, new
					{
						ID = aggegateID,
						Sequence = change.SequenceID,
 						Event = JsonConvert.SerializeObject(change, _jsonSettings)
					});
				}
			}
		}

		public IEnumerable<DomainEvent> LoadEvents(TKey aggegateID)
		{
			var sql = "select event from events_guid where aggregateID = @id order by sequence asc";

			using (var connection = Open())
			{
				return connection
					.Query<string>(sql, new { ID = aggegateID})
					.Select(json => JsonConvert.DeserializeObject<DomainEvent>(json, _jsonSettings));
			}
		}

		public IEnumerable<DomainEvent> LoadEventsSince(TKey aggegateID, int sequenceID)
		{
			throw new System.NotImplementedException();
		}

		public ISequenced LoadLatestSnapshotFor(TKey aggegateID)
		{
			throw new System.NotImplementedException();
		}

		public void SaveSnapshot(TKey aggregateID, ISequenced snapshot)
		{
			throw new System.NotImplementedException();
		}
	}
}
