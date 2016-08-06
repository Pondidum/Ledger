using System;
using System.Collections.Generic;
using Ledger.Acceptance.TestDomain.Events;
using Ledger.Projections;
using Shouldly;
using Xunit;

namespace Ledger.Tests.Projections
{
	public class ProjectorTests
	{
		[Fact]
		public void When_nothing_is_registered()
		{
			var projection = new EmptyProjection();

			Should.NotThrow(() => projection.Apply(new CandidateCreated()));
		}

		[Fact]
		public void When_one_event_is_registered_and_it_is_applied()
		{
			var sent = new CandidateCreated { AggregateID = Guid.NewGuid() };

			var projection = new CreateOnlyProjection();
			projection.Apply(sent);

			projection.Events.ShouldBe(new[] { sent });
		}

		[Fact]
		public void When_one_event_is_registered_and_it_is_not_applied()
		{
			var sent = new AddEmailAddress { AggregateID = Guid.NewGuid() };

			var projection = new CreateOnlyProjection();
			projection.Apply(sent);

			projection.Events.ShouldBeEmpty();
		}

		[Fact]
		public void When_an_inherited_event_is_registered_and_applied()
		{
			var sent = new ChildEvent();

			var projection = new ParentOnlyProjection();
			projection.Apply(sent);

			projection.Events.ShouldBe(new[] { sent });
		}

		[Fact]
		public void When_an_inherited_event_is_registered_directly_and_inherited()
		{
			var sent = new ChildEvent();

			var projection = new ParentAndChildProjection();
			projection.Apply(sent);

			projection.ParentEvents.ShouldBeEmpty();
			projection.ChildEvents.ShouldBe(new[] { sent });
		}

		private class ParentEvent : DomainEvent<Guid> { }
		private class ChildEvent : ParentEvent { }

		private class EmptyProjection : Projection
		{
		}

		private class CreateOnlyProjection : Projection
		{
			public List<DomainEvent<Guid>> Events { get; set; } = new List<DomainEvent<Guid>>();

			private void Handle(CandidateCreated e)
			{
				Events.Add(e);
			}
		}

		private class ParentOnlyProjection : Projection
		{
			public List<DomainEvent<Guid>> Events { get; set; } = new List<DomainEvent<Guid>>();

			private void Handle(ParentEvent e)
			{
				Events.Add(e);
			}
		}

		private class ParentAndChildProjection : Projection
		{
			public List<DomainEvent<Guid>> ParentEvents { get; set; } = new List<DomainEvent<Guid>>();
			public List<DomainEvent<Guid>> ChildEvents { get; set; } = new List<DomainEvent<Guid>>();

			private void Handle(ParentEvent e)
			{
				ParentEvents.Add(e);
			}

			private void Handle(ChildEvent e)
			{
				ChildEvents.Add(e);
			}
		}
	}
}