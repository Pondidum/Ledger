using System;
using System.Collections.Generic;
using System.Linq;
using Ledger.Infrastructure;

namespace Ledger
{
	public class AggregateRoot<TKey>
	{
		public TKey ID { get; protected set; }
		public int SequenceID { get; protected set; }

		private readonly LightweightCache<Type, List<Action<IDomainEvent>>> _handlers;
		private readonly List<IDomainEvent> _events;

		protected AggregateRoot()
		{
			_handlers = new LightweightCache<Type, List<Action<IDomainEvent>>>(
				key => new List<Action<IDomainEvent>>()
			);

			_events = new List<IDomainEvent>();
			SequenceID = -1;
		}

		public IEnumerable<IDomainEvent> GetUncommittedEvents()
		{
			return _events;
		}

		internal void MarkEventsCommitted()
		{
			if (_events.Any())
			{
				SequenceID = _events.Last().Sequence;
				_events.Clear();
			}
		}

		internal void LoadFromEvents(IEnumerable<IDomainEvent> eventStream)
		{
			IDomainEvent last = null;
			var dynamic = this.AsDynamic();

			eventStream
				.Apply(e => last = e)
				.ForEach(e => dynamic.Handle(e));

			if (last != null)
			{
				SequenceID = last.Sequence;
			}
		}

		internal void LoadFromSnapshot<TSnapshot>(TSnapshot snapshot, IEnumerable<IDomainEvent> events)
			where TSnapshot : ISequenced
		{
			if (snapshot != null)
			{
				this.AsDynamic().ApplySnapshot(snapshot);
				SequenceID = snapshot.Sequence;
			}

			LoadFromEvents(events);
		}

		protected void BeforeApplyEvent<TEvent>(Action<TEvent> handler)
			where TEvent : IDomainEvent
		{
			_handlers[typeof(TEvent)].Add(e => handler((TEvent)e));
		}

		protected void ApplyEvent(IDomainEvent @event)
		{
			var eventType = @event.GetType();

			_handlers
				.Dictionary
				.Where(p => p.Key.IsAssignableFrom(eventType))
				.SelectMany(p => p.Value)
				.ForEach(handler => handler.Invoke(@event));

			this.AsDynamic().Handle(@event);
			_events.Add(@event);
		}
	}
}
