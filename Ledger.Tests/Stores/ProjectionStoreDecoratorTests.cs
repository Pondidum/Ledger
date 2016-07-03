using System;
using System.Collections.Generic;
using System.Linq;
using Ledger.Acceptance.TestDomain;
using Ledger.Acceptance.TestDomain.Events;
using Ledger.Stores;
using Shouldly;
using Xunit;

namespace Ledger.Tests.Stores
{
	public class ProjectionStoreDecoratorTests
	{
		[Fact]
		public void When_colleting_all_events()
		{
			var projectionist = new Projectionist();
			var memoryStore = new InMemoryEventStore();
			var wrapped = new ProjectionStoreDecorator(memoryStore, projectionist);

			var aggregateStore = new AggregateStore<Guid>(wrapped);

			var user = Candidate.Create("test", "test@home.com");
			aggregateStore.Save("Users", user);

			memoryStore.AllEvents.Single().ShouldBeOfType<CandidateCreated>();
			projectionist.SeenEvents.Single().ShouldBeOfType<CandidateCreated>();
		}

		private class Projectionist : IProjectionist
		{
			private readonly List<DomainEvent<Guid>> _seenEvents;
			 
			public Projectionist()
			{
				_seenEvents = new List<DomainEvent<Guid>>();
			}

			public IEnumerable<DomainEvent<Guid>> SeenEvents => _seenEvents;
			 
			public void Project<TKey>(DomainEvent<TKey> domainEvent)
			{
				_seenEvents.Add(domainEvent as DomainEvent<Guid>);
			}
		}
	}
}
