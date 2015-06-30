using Respawn;

namespace Ledger.Stores.Postgres.Tests
{
	public class PostgresTestBase
	{
		public string ConnectionString { get; set; }

		public PostgresTestBase()
		{
			ConnectionString = "PORT=5432;TIMEOUT=15;POOLING=True;MINPOOLSIZE=1;MAXPOOLSIZE=20;COMMANDTIMEOUT=20;COMPATIBLE=2.1.3.0;HOST=192.168.59.103;USER ID=postgres;DATABASE=postgres";
		}

		protected static Checkpoint Checkpoint = new Checkpoint
		{
			SchemasToInclude = new[] { "postgres"},
			DbAdapter = DbAdapter.Postgres
		};
	}
}
