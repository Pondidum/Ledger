namespace Ledger.Stores
{
	public class InterceptingEventStore : IEventStore
	{
		private readonly IEventStore _other;

		public InterceptingEventStore(IEventStore other)
		{
			_other = other;
		}

		public virtual IStoreReader<TKey> CreateReader<TKey>(EventStoreContext context)
		{
			return new InterceptingReader<TKey>(_other.CreateReader<TKey>(context));
		}

		public virtual IStoreWriter<TKey> CreateWriter<TKey>(EventStoreContext context)
		{
			return new InterceptingWriter<TKey>(_other.CreateWriter<TKey>(context));
		}

		public IStoreMaintainer<TKey> CreateMaintainer<TKey>(EventStoreContext context)
		{
			return new InterceptingMaintainer<TKey>(_other.CreateMaintainer<TKey>(context));
		}
	}
}
