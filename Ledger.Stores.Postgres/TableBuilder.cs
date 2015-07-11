using System;
using System.Collections.Generic;
using System.Linq;
using Npgsql;

namespace Ledger.Stores.Postgres
{
	public class TableBuilder
	{
		private readonly Dictionary<Type, Action> _creators;

		public TableBuilder(NpgsqlConnection connection, ITableName tableName)
		{
			_creators = new Dictionary<Type, Action>
			{
				{typeof (Guid), () => new CreateGuidAggregateTablesCommand(connection, tableName).Execute()},
				{typeof (int), () => new CreateIntAggregateTablesCommand(connection, tableName).Execute()}
			};
		}

		public void CreateTable<TKey>()
		{
			Action create;

			if (_creators.TryGetValue(typeof (TKey), out create))
			{
				create();
			}

			throw new NotSupportedException(string.Format(
				"Cannot create a '{0}' aggregate keyed table, only '{1}' are supported.", 
				typeof(TKey).Name,
				string.Join(", ", _creators.Keys.Select(k => k.Name))));
		}
	}
}
