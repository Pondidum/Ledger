using System;
using System.Collections.Generic;
using System.Linq;
using Ledger;
using TestsDomain.Events;

namespace TestsDomain
{
	public class Candidate : AggregateRoot<Guid>, ISnapshotable<CandidateMemento>
	{
		public string Name { get; private set; }
		public IEnumerable<string> Emails { get { return _emails; } }

		private readonly List<string> _emails;

		public Candidate()
		{
			_emails = new List<string>();
		}

		public static Candidate Create(string name, string email)
		{
			var candidate = new Candidate();
			candidate.ApplyEvent(new CandidateCreated
			{
				CandidateID = Guid.NewGuid(),
				CandidateName = name,
				EmailAddress = email
			});

			return candidate;
		}

		public CandidateMemento CreateSnapshot()
		{
			return new CandidateMemento
			{
				Name = Name,
				Emails = _emails
			};
		}

		public void ApplySnapshot(CandidateMemento snapshot)
		{
			Name = snapshot.Name;
			_emails.Clear();
			_emails.AddRange(snapshot.Emails);
		}

		public void FixName(string newName)
		{
			ApplyEvent(new FixNameSpelling { NewName = newName });
		}

		public void NameChangedByDeedPoll(string newName)
		{
			ApplyEvent(new NameChangedByDeedPoll { NewName = newName });
		}

		public void AddNewEmail(string email)
		{
			if (_emails.Any(e => string.Equals(e, email, StringComparison.OrdinalIgnoreCase)))
			{
				throw new DomainException(string.Format("A {0} cannot have duplicate emails.", GetType().Name));
			}

			ApplyEvent(new AddEmailAddress { Email = email});
		}

		private void Handle(CandidateCreated @event)
		{
			ID = @event.CandidateID;
			Name = @event.CandidateName;
			_emails.Add(@event.EmailAddress);
		}

		private void Handle(FixNameSpelling @event)
		{
			Name = @event.NewName;
		}

		private void Handle(NameChangedByDeedPoll @event)
		{
			Name = @event.NewName;
		}

		private void Handle(AddEmailAddress e)
		{
			_emails.Add(e.Email);
		}

	}
}
