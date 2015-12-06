using System;
using System.Collections.Generic;
using System.Linq;
using Ledger.Acceptance.TestObjects;
using Ledger.Infrastructure;
using Ledger.Stores;
using Shouldly;
using Xunit;

namespace Ledger.Tests.Stores
{
	public class InterceptingEventStoreTests
	{

		[Fact]
		public void When_an_event_occours()
		{
			var events = new List<IDomainEvent<Guid>>();
			var store = new InMemoryEventStore();
			var wrapped = new LoggingEventStore(store, e => events.Add(e));

			var aggregate = new TestAggregate(DefaultStamper.Now);
			aggregate.GenerateID();

			var event1 = new TestEvent();
			aggregate.AddEvent(event1);

			var ags = new AggregateStore<Guid>(wrapped);
			ags.Save("testStream", aggregate);

			events.ShouldBe(new[] { event1 });
			store.AllEvents.Cast<IDomainEvent<Guid>>().ShouldBe(new[] { event1 });

		}


		private class LoggingEventStore : InterceptingEventStore
		{
			private readonly IEventStore _other;
			private readonly Action<IDomainEvent<Guid>> _onEvent;

			public LoggingEventStore(IEventStore other, Action<IDomainEvent<Guid>> onEvent)
				: base(other)
			{
				_other = other;
				_onEvent = onEvent;
			}

			public override IStoreWriter<TKey> CreateWriter<TKey>(string stream)
			{
				return new EventLoggingStoreWriter<TKey>(_other.CreateWriter<TKey>(stream), e =>  _onEvent((IDomainEvent<Guid>) e) );
			}
		}

		private class EventLoggingStoreWriter<TKey> : InterceptingWriter<TKey>
		{
			private readonly Action<IDomainEvent<TKey>> _onEvent;

			public EventLoggingStoreWriter(IStoreWriter<TKey> other, Action<IDomainEvent<TKey>> onEvent)
				: base(other)
			{
				_onEvent = onEvent;
			}

			public override void SaveEvents(IEnumerable<IDomainEvent<TKey>> changes)
			{
				//this is a pretty bad impl, as you can block event saving, but its easy to test!
				//also using the .Apply() method avoids iterating the changes collection more than once.

				base.SaveEvents(changes.Apply(change => _onEvent(change)));
			}
		}
	}
}
