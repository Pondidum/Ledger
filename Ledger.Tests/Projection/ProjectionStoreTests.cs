using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Ledger.Acceptance.TestDomain;
using Ledger.Acceptance.TestDomain.Events;
using Ledger.Projection;
using Ledger.Stores;
using Shouldly;
using Xunit;

namespace Ledger.Tests.Projection
{
	public class ProjectionStoreTests
	{
		[Fact]
		public void When_colleting_all_events()
		{
			var reset  = new AutoResetEvent(false);

			var projectionist = new Projectionist(reset);
			var memoryStore = new InMemoryEventStore();
			var wrapped = new ProjectionStore(memoryStore, config =>
			{
				config.ProjectTo(projectionist);
			});

			var aggregateStore = new AggregateStore<Guid>(wrapped);

			var user = Candidate.Create("test", "test@home.com");
			aggregateStore.Save("Users", user);

			reset.WaitOne();

			memoryStore.AllEvents.Single().ShouldBeOfType<CandidateCreated>();
			projectionist.SeenEvents.Single().ShouldBeOfType<CandidateCreated>();
		}

		private class Projectionist : IProjectionist
		{
			private readonly AutoResetEvent _reset;
			private readonly List<DomainEvent<Guid>> _seenEvents;
			 
			public Projectionist(AutoResetEvent reset)
			{
				_reset = reset;
				_seenEvents = new List<DomainEvent<Guid>>();
			}

			public IEnumerable<DomainEvent<Guid>> SeenEvents => _seenEvents;
			 
			public void Apply<TKey>(DomainEvent<TKey> domainEvent)
			{
				_seenEvents.Add(domainEvent as DomainEvent<Guid>);
				_reset.Set();
			}
		}
	}
}
