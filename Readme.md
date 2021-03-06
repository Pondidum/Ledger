# Ledger

Ledger is a lightweight eventsourcing library for .net.

## Configuration

First you need to configure your AggregateStore with a backing event store.  In this example we are using the filesystem store (provided by [Ledger.Stores.Fs][nuget-ledger-store-fs]).  There are also packages available for [Postgres][nuget-ledger-store-postgres] based event stores, and a built in `InMemoryEventStore`.

```c#
var eventStore = new FileEventStore<Guid>("..\\appdata\\eventstore");
var aggregateStore = new AggregateStore<Guid>(eventStore);
```

To load an entity from the aggregateStore, you need to provide it with an `id`, and a lambda which creates a blank entity for the aggregateStore to populate:

```c#
var person = aggregateStore.Load(id, () => new Person());
```

You can save any changes to the entity later by calling the `Save` method on the aggregateStore:

```c#
aggregateStore.Save(person);
```

## Usage: AggregateStore

Under Webapi, you can use a DI Container to provide the aggregateStore to your controllers.  For example (using structuremap):

```c#
public class WebApiConfig
{
	public void Configure(HttpConfiguration http)
	{
		var container = new Container(config =>
		{
			config.Scan(asm => {
				asm.TheCallingAssembly();
				asm.WithDefaultConventions();
			});

			config
				.For<NpgsqlConnection>()
				.Use(() => new NpgsqlConnection(ConfigurationManager.ConnectionString["Postgres"]));

			config
				.For<IEventStore<Guid>>()
				.Use<PostgresEventStore<Guid>>();
		});

		http.DependencyResolver = new StructureMapDependencyResolver(container);
	}
}


public class AccountController : ApiController
{
	private readonly IEventStore<Guid> _eventStore;

	public AccountController(IEventStore<Guid> eventStore)
	{
		_eventStore = eventstore;
	}
}
```




## Usage: Aggregates

A basic event sourced entity needs to inherit from `AggregateRoot`, and specify the type of key to use.  Currently only `int` and `Guid` are supported.

All actions performed on your AggregateRoot get split into two parts; validation and application.  In the following example we have an `AddNewEmail` method which does some checks (hint: business rules go here) and then creates the event.  A second method called `Handle` applies the event to the aggregate.  These `Handle` methods get called when an aggregate is loaded from an event stream too.

```c#
public class Person : AggregateRoot<Guid>
{
	public IEnumerable<string> Emails { get { return _emails; } }

	private readonly HashSet<string> _emails;

	public Person()
	{
		_emails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
	}

	public void AddNewEmail(string email)
	{
		if (_emails.Contains(email))
		{
			throw new DomainException(string.Format("A {0} cannot have duplicate emails.", GetType().Name));
		}

		ApplyEvent(new AddEmailAddress { Email = email});
	}

	private void Handle(AddEmailAddress e)
	{
		_emails.Add(e.Email);
	}
}
```

You have one `Handle` method per event type, for example to add a `RemoveEmail` method, you would do this:

```c#
public void RemoveEmail(string email)
{
	if (_emails.Contains(email))
	{
		ApplyEvent(new RemoveEmailAddress { Email = email });
	}
}

private void Handle(RemoveEmailAddress e)
{
	_emails.Remove(e.Email);
}
```

## Usage: Projections

If you wish to make use of projections, you can decorate the `IEventStore` of your choice with the `ProjectionStoreDecorator`, passing in an implementation of `IProjectionist`:

```c#
var eventStore = new FileEventStore<Guid>("..\\appdata\\eventstore");
var wrapped = new ProjectionStoreDecorator(eventStore, projectionist)
```

```c#
public class ProjectionDispatcher : IProjectionist
{
	private readonly Projector<Guid> _projector;

	public ProjectionDispatcher()
	{
		_projector = new Projector<Guid>();

		_projector.Register<RemoveEmailAddress>(e => { /* do something with the event */ });
	}

	public void Project<TKey>(DomainEvent<TKey> domainEvent)
	{
		_projector.Apply(domainEvent as DomainEvent<Guid>);
	}
}
```

# Projections

When using projecitons you will often need them to be persistent - and to resume processing events from where they left off (in the event of service restarts/crashes etc.)

There are two main options for doing this, either in-process or out-of-process.  Personally I prefer out-of-process as I can have multiple services doing projections, or move the projection service to a separate host if resource usage becomes an issue.

## In Process

The readmodel building service is fairly straight forward.  On startup

1. get all events since the last seen, store in `_preload` (just an `IEnumerable<DomainEvent<T>>`)
2. start listening to new events, appending to `_events` (which is a `BlockingCollection<DomainEvent<T>>`)
3. start the processing task which:
	1. processes all events in `_preload`
	2. enters infinite loop of poping events off `_events` and processing them

The two important methods to this class are as follows:

```CSharp
public void Start(string streamName, StreamSequence lastSeen)
{
	_lastSeen = lastSeen;
	_preload = _store.ReplayAllSince(streamName, _lastSeen);

	Task.Run(() => Process(), _task.Token);
}

private void Process()
{
	foreach (var e in _preload)
	{
		_projector.Apply(e);
		_lastSeen = e.StreamSequence;
	}

	while (_task.IsCancellationRequested == false)
	{
		var e = _events.Take();
		_projector.Apply(e);
		_lastSeen = e.StreamSequence;
	}
}
```

The only other thing to bear in mind with this method is that you must store the `_lastSeen` value somewhere where it will survive process restarts.  It could be written to disk, or as an extra value in wherever your readmodel is being stored.

## Out Of Process

To ensure no events are missed when using an Out Of Process projector, I use a queuing/messagebroker, such as RabbitMQ.  The queue we use within RabbitMQ only removes a message from the tip of the queue when it's acknowledged, so service/restart resume code is not required.

To send messages to RabbitMQ we implement an `IEventStore` decorator which forwards messages.

```CSharp
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
```

To process the messages, we write a second application which listens to the queue:

```CSharp
using (var connetion = factory.CreateConnection())
using (var model = connetion.CreateModel())
{
	var listener = new EventingBasicConsumer(model);

	listener.Received += (s, e) =>
	{
		var domainEvent = Serializer.Deserialize<TestEvent>(Encoding.UTF8.GetString(e.Body));

		_projector.Apply(domainEvent);

		model.BasicAck(e.DeliveryTag, multiple: false);
	};

	model.BasicConsume(QueueName, noAck: false, consumer: listener);
}
```

[nuget-ledger-store-fs]: https://www.nuget.org/packages/Ledger.Stores.Fs/
[nuget-ledger-store-postgres]: https://www.nuget.org/packages/Ledger.Stores.Postgres/
