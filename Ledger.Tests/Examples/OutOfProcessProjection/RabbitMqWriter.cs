using System.Collections.Generic;
using System.Text;
using Ledger.Infrastructure;
using Ledger.Stores;
using RabbitMQ.Client;

namespace Ledger.Tests.Examples.OutOfProcessProjection
{
	public class RabbitMqWriter<TKey> : InterceptingWriter<TKey>
	{
		private readonly IConnectionFactory _factory;
		private readonly string _queueName;
		private readonly IConnection _connection;
		private readonly IModel _model;

		public RabbitMqWriter(IStoreWriter<TKey> other, IConnectionFactory factory, string queueName) : base(other)
		{
			_factory = factory;
			_queueName = queueName;
			_connection = _factory.CreateConnection();
			_model = _connection.CreateModel();
		}

		public override void SaveEvents(IEnumerable<DomainEvent<TKey>> changes)
		{
			base.SaveEvents(changes.Apply(SendToRabbit));
		}

		private void SendToRabbit(DomainEvent<TKey> domainEvent)
		{
			var queue = _model.QueueDeclare(_queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
			var body = Encoding.UTF8.GetBytes(Serializer.Serialize(domainEvent));

			_model.BasicPublish("", queue.QueueName, _model.CreateBasicProperties(), body);
		}

		public override void Dispose()
		{
			base.Dispose();

			_model.Dispose();
			_connection.Dispose();
		}
	}
}