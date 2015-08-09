# Ledger

Ledger is a lightweight eventsourcing library for .net.

## Usage

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
