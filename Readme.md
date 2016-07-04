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


[nuget-ledger-store-fs]: https://www.nuget.org/packages/Ledger.Stores.Fs/
[nuget-ledger-store-postgres]: https://www.nuget.org/packages/Ledger.Stores.Postgres/
