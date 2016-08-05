using System;
using Ledger.Acceptance.TestDomain.Events;
using Ledger.Projection;
using Shouldly;
using Xunit;

namespace Ledger.Tests.Projection
{
	public class ProjectorTests
	{
		private readonly Projector<Guid> _projector;

		public ProjectorTests()
		{
			_projector = new Projector<Guid>();
		}

		[Fact]
		public void When_nothing_is_registered()
		{
			Should.NotThrow(() => _projector.Apply(new CandidateCreated()));

			_projector.RegisteredEvents.ShouldBeEmpty();
		}

		[Fact]
		public void When_one_event_is_registered_and_it_is_applied()
		{
			var sent = new CandidateCreated { AggregateID = Guid.NewGuid() };
			DomainEvent<Guid> projected = null;

			_projector.Register<CandidateCreated>(e => projected = e);
			_projector.Apply(sent);

			projected.ShouldBe(sent);
		}

		[Fact]
		public void When_one_event_is_registered_and_it_is_not_applied()
		{
			var sent = new  AddEmailAddress { AggregateID = Guid.NewGuid() };
			DomainEvent<Guid> projected = null;

			_projector.Register<CandidateCreated>(e => projected = e);
			_projector.Apply(sent);

			projected.ShouldBeNull();
		}

		[Fact]
		public void When_an_inherited_event_is_registered_and_applied()
		{
			var sent = new ChildEvent();
			DomainEvent<Guid> projected = null;

			_projector.Register<ParentEvent>(e => projected = e);
			_projector.Apply(sent);

			projected.ShouldBe(sent);
		}

		[Fact]
		public void When_an_inherited_event_is_registered_directly_and_inherited()
		{
			var sent = new ChildEvent();
			DomainEvent<Guid> projectedParent = null;
			DomainEvent<Guid> projectedChild = null;

			_projector.Register<ParentEvent>(e => projectedParent = e);
			_projector.Register<ChildEvent>(e => projectedChild= e);
			_projector.Apply(sent);

			projectedParent.ShouldBe(sent);
			projectedChild.ShouldBe(sent);
		}

		private class ParentEvent : DomainEvent<Guid>
		{
			
		}

		private class ChildEvent : ParentEvent
		{
			
		} 

	}
}