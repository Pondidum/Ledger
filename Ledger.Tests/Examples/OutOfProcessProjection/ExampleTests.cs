using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Ledger.Acceptance.TestObjects;
using Ledger.Infrastructure;
using Ledger.Stores;
using Ledger.Tests.TestInfrastructure;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shouldly;
using Xunit;

namespace Ledger.Tests.Examples.OutOfProcessProjection
{
	public class ExampleTests
	{
		private const string StreamName = "TestStream";
		private const string QueueName = "DomainEvents";

		[RequiresRabbitFact]
		public void When_reading()
		{
			var factory = new ConnectionFactory { HostName = "10.0.75.2" };


			using (var connetion = factory.CreateConnection())
			using (var model = connetion.CreateModel())
			{
				model.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
				model.QueuePurge(QueueName);
			}

			var backing = new InMemoryEventStore();
			var wrapped = new RabbitMqStoreDecorator(backing, factory, QueueName);
			var store = new AggregateStore<Guid>(wrapped);

			var id = Guid.NewGuid();

			using (var writer = wrapped.CreateWriter<Guid>(store.CreateContext(StreamName)))
				writer.SaveEvents(new[]
				{
					new TestEvent { AggregateID = id, Sequence = new Sequence(0) },
					new TestEvent { AggregateID = id, Sequence = new Sequence(1) },
					new TestEvent { AggregateID = id, Sequence = new Sequence(2) },
					new TestEvent { AggregateID = id, Sequence = new Sequence(3) },
					new TestEvent { AggregateID = id, Sequence = new Sequence(4) },
				});

			// end setup

			var wait = new AutoResetEvent(false);
			var seen = new List<Sequence>();

			using (var connetion = factory.CreateConnection())
			using (var model = connetion.CreateModel())
			{
				var listener = new EventingBasicConsumer(model);

				listener.Received += (s, e) =>
				{
					var domainEvent = Serializer.Deserialize<TestEvent>(Encoding.UTF8.GetString(e.Body));
					seen.Add(domainEvent.Sequence);

					model.BasicAck(e.DeliveryTag, multiple: false);

					if (domainEvent.Sequence == new Sequence(4))
						wait.Set();
				};

				model.BasicConsume(QueueName, noAck: false, consumer: listener);

				wait.WaitOne();
			}

			seen.ShouldBe(new[]
			{
				new Sequence(0),
				new Sequence(1),
				new Sequence(2),
				new Sequence(3),
				new Sequence(4)
			});
		}
	}
}
