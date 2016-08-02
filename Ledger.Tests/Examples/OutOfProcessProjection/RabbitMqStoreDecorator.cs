using RabbitMQ.Client;

namespace Ledger.Tests.Examples.OutOfProcessProjection
{
	public class RabbitMqStoreDecorator : IEventStore
	{
		private readonly IEventStore _other;
		private readonly IConnectionFactory _factory;
		private readonly string _queueName;

		public RabbitMqStoreDecorator(IEventStore other, IConnectionFactory factory, string queueName)
		{
			_other = other;
			_factory = factory;
			_queueName = queueName;
		}

		public IStoreReader<TKey> CreateReader<TKey>(EventStoreContext context)
		{
			return _other.CreateReader<TKey>(context);
		}

		public IStoreWriter<TKey> CreateWriter<TKey>(EventStoreContext context)
		{
			return new RabbitMqWriter<TKey>(
				_other.CreateWriter<TKey>(context),
				_factory,
				_queueName
				);
		}
	}
}