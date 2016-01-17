namespace Ledger.Stores
{
	public class InterceptingEventStore : IEventStore
	{
		private readonly IEventStore _other;

		public InterceptingEventStore(IEventStore other)
		{
			_other = other;
		}

		public virtual IStoreReader<TKey> CreateReader<TKey>(string stream)
		{
			return new InterceptingReader<TKey>(_other.CreateReader<TKey>(stream));
		}

		public virtual IStoreWriter<TKey> CreateWriter<TKey>(string stream)
		{
			return new InterceptingWriter<TKey>(_other.CreateWriter<TKey>(stream));
		}

		public IStoreMaintainer<TKey> CreateMaintainer<TKey>(string streamName)
		{
			return new InterceptingMaintainer<TKey>(_other.CreateMaintainer<TKey>(streamName));
		}
	}
}
