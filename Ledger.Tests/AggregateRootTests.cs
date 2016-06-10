using System;
using Shouldly;
using Xunit;

namespace Ledger.Tests
{
	public class AggregateRootTests
	{
		[Fact]
		public void When_there_is_no_pre_processor()
		{
			var aggregate = new PreProcessAggregate();

			var e = new PreProssesedEvent() { Name = "Test" };
			aggregate.PushEvent(e);

			e.Name.ShouldBe("Test");
		}

		[Fact]
		public void When_there_is_a_specific_event_pre_processor()
		{
			var aggregate = new PreProcessAggregate();

			aggregate.AddPreProcessor<PreProssesedEvent>(p => p.Name = "applied");

			var e = new PreProssesedEvent();
			aggregate.PushEvent(e);

			e.Name.ShouldBe("applied");
		}

		[Fact]
		public void When_there_is_a_generic_event_pre_processor()
		{
			var aggregate = new PreProcessAggregate();

			aggregate.AddPreProcessor<BaseEvent>(p => p.Key = "keyed");

			var e1 = new PreProssesedEvent();
			aggregate.PushEvent(e1);

			var e2 = new NonProcessedEvent();
			aggregate.PushEvent(e2);

			e1.Key.ShouldBe("keyed");
			e2.Key.ShouldBe("keyed");
		}

		[Fact]
		public void When_there_is_no_handler_for_a_domainEvent()
		{
			var aggregate = new NonHandlingAggregate();
			
			Should
				.Throw<MissingMethodException>(() => aggregate.PushEvent(new NonProcessedEvent()))
				.Message
				.ShouldContain(nameof(NonProcessedEvent));
		}




		private class PreProcessAggregate : AggregateRoot<Guid>
		{
			public PreProcessAggregate()
			{
			}

			public void AddPreProcessor<TEvent>(Action<TEvent> handler)
				where TEvent : DomainEvent<Guid>
			{
				BeforeApplyEvent(handler);
			}

			public void PushEvent<TEvent>(TEvent @event) where TEvent : DomainEvent<Guid>
			{
				ApplyEvent(@event);
			}

			private void Handle(PreProssesedEvent e)
			{
			}

			private void Handle(NonProcessedEvent e)
			{
			}
		}

		private class NonHandlingAggregate : AggregateRoot<Guid>
		{
			public void PushEvent(DomainEvent<Guid> domainEvent)
			{
				ApplyEvent(domainEvent);
			}
		}

		private class BaseEvent : DomainEvent<Guid>
		{
			public string Key { get; set; }
		}

		private class PreProssesedEvent : BaseEvent
		{
			public string Name { get; set; }
		}

		private class NonProcessedEvent : BaseEvent
		{
			public string Name { get; set; }
		}

	}
}
