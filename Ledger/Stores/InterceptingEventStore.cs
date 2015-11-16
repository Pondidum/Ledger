namespace Ledger.Stores
{
	public class InterceptingEventStore: IEventStore
	{
		private readonly IEventStore _other;

		public InterceptingEventStore(IEventStore other)
		{
			_other = other;
		}

		public virtual IStoreReader<TKey> CreateReader<TKey>()
		{
			return new InterceptingReader<TKey>(_other.CreateReader<TKey>());
		}

		public virtual IStoreWriter<TKey> CreateWriter<TKey>()
		{
			return new InterceptingWriter<TKey>(_other.CreateWriter<TKey>());
		}
	}
}
