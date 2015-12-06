using System;
using System.Collections.Generic;
using System.Linq;
using Ledger.Infrastructure;

namespace Ledger
{
	public class AggregateRoot<TKey>
	{
		public TKey ID { get; protected set; }
		protected internal int SequenceID { get; set; }

		private readonly LightweightCache<Type, List<Action<IDomainEvent<TKey>>>> _handlers;
		private readonly List<IDomainEvent<TKey>> _events;

		protected AggregateRoot()
		{
			_handlers = new LightweightCache<Type, List<Action<IDomainEvent<TKey>>>>(
				key => new List<Action<IDomainEvent<TKey>>>()
			);

			_events = new List<IDomainEvent<TKey>>();
			SequenceID = -1;
		}

		public IEnumerable<IDomainEvent<TKey>> GetUncommittedEvents()
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

		internal void LoadFromEvents(IEnumerable<IDomainEvent<TKey>> eventStream)
		{
			IDomainEvent<TKey> last = null;
			var dynamic = this.AsDynamic();

			eventStream
				.Apply(e => last = e)
				.ForEach(e => dynamic.Handle(e));

			if (last != null)
			{
				SequenceID = last.Sequence;
			}
		}

		internal void LoadFromSnapshot<TSnapshot>(TSnapshot snapshot, IEnumerable<IDomainEvent<TKey>> events)
			where TSnapshot : ISnapshot<TKey>
		{
			if (snapshot != null)
			{
				this.AsDynamic().ApplySnapshot(snapshot);
				SequenceID = snapshot.Sequence;
				ID = snapshot.AggregateID;
			}

			LoadFromEvents(events);
		}

		protected void BeforeApplyEvent<TEvent>(Action<TEvent> handler)
			where TEvent : IDomainEvent<TKey>
		{
			_handlers[typeof(TEvent)].Add(e => handler((TEvent)e));
		}

		protected void ApplyEvent(IDomainEvent<TKey> @event)
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
