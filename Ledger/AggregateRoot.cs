using System;
using System.Collections.Generic;
using System.Linq;
using Ledger.Infrastructure;

namespace Ledger
{
	public class AggregateRoot<TKey>
	{
		public TKey ID { get; protected set; }
		protected internal DateTime SequenceID { get; set; }

		private readonly Func<DateTime> _getTimestamp;
		private readonly LightweightCache<Type, List<Action<DomainEvent<TKey>>>> _handlers;
		private readonly List<DomainEvent<TKey>> _events;

		protected AggregateRoot()
			: this(DefaultStamper.Now)
		{
		}

		protected AggregateRoot(Func<DateTime> getTimestamp)
		{
			_getTimestamp = getTimestamp;
			_handlers = new LightweightCache<Type, List<Action<DomainEvent<TKey>>>>(
				key => new List<Action<DomainEvent<TKey>>>()
			);

			_events = new List<DomainEvent<TKey>>();
			SequenceID = DateTime.MinValue;

			BeforeApplyEvent<DomainEvent<TKey>>(e => e.Stamp = _getTimestamp());
		}

		public IEnumerable<DomainEvent<TKey>> GetUncommittedEvents()
		{
			return _events;
		}

		internal void MarkEventsCommitted()
		{
			if (_events.Any())
			{
				SequenceID = _events.Last().Stamp;
				_events.Clear();
			}
		}

		internal void LoadFromEvents(IEnumerable<DomainEvent<TKey>> eventStream)
		{
			DomainEvent<TKey> last = null;
			var dynamic = this.AsDynamic();

			eventStream
				.Apply(e => last = e)
				.ForEach(e => dynamic.Handle(e));

			if (last != null)
			{
				SequenceID = last.Stamp;
			}
		}

		internal void LoadFromSnapshot<TSnapshot>(TSnapshot snapshot, IEnumerable<DomainEvent<TKey>> events)
			where TSnapshot : ISnapshot<TKey>
		{
			if (snapshot != null)
			{
				this.AsDynamic().ApplySnapshot(snapshot);
				SequenceID = snapshot.Stamp;
				ID = snapshot.AggregateID;
			}

			LoadFromEvents(events);
		}

		protected void BeforeApplyEvent<TEvent>(Action<TEvent> handler)
			where TEvent : DomainEvent<TKey>
		{
			_handlers[typeof(TEvent)].Add(e => handler((TEvent)e));
		}

		protected void ApplyEvent(DomainEvent<TKey> @event)
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
