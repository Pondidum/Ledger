using Newtonsoft.Json;

namespace Ledger
{
	public class LedgerConfiguration
	{
		public IEventStore EventStore { get; set; }
		public ITypeResolver TypeResolver { get; set; }
		public SnapshotPolicy SnapshotPolicy { get; set; }
		public JsonSerializerSettings SerializerSettings { get; set; }
	}
}
