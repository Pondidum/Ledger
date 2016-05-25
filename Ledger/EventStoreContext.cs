namespace Ledger
{
	public class EventStoreContext
	{
		public string StreamName { get; }
		public ITypeResolver TypeResolver { get; }

		public EventStoreContext(string streamName, ITypeResolver typeResolver)
		{
			StreamName = streamName;
			TypeResolver = typeResolver;
		}
	}
}
