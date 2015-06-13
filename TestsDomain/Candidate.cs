﻿using System;
using System.Collections.Generic;
using Ledger;
using TestsDomain.Events;

namespace TestsDomain
{
	public class Candidate : AggregateRoot<Guid>
	{
		public string Name { get; private set; }
		public IEnumerable<string> Emails { get { return _emails; } }

		private readonly List<string> _emails;

		private Candidate()
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

		private void Handle(CandidateCreated @event)
		{
			ID = @event.CandidateID;
			Name = @event.CandidateName;
			_emails.Add(@event.EmailAddress);
		}
	}
}
