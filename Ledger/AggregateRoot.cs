using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Ledger.Infrastructure;

namespace Ledger
{
	public class AggregateRoot<TKey>
	{
		public TKey ID { get; protected set; }
		protected internal DateTime Stamp { get; set; }
		protected internal Sequence Sequence { get; set; }

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
			Stamp = DateTime.MinValue;
			Sequence = Sequence.Start;

			//BeforeApplyEvent<DomainEvent<TKey>>(e => e.Sequence = ++Sequence);
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
				Stamp = _events.Last().Stamp;
				Sequence = _events.Last().Sequence;
				_events.Clear();
			}
		}

		public void LoadFromEvents(IEnumerable<DomainEvent<TKey>> eventStream)
		{
			DomainEvent<TKey> last = null;
			var dynamic = this.AsDynamic();

			eventStream
				.Apply(e => last = e)
				.ForEach(e => dynamic.Handle(e));

			if (last != null)
			{
				Stamp = last.Stamp;
				Sequence = last.Sequence;
			}
		}

		public void LoadFromSnapshot<TSnapshot>(TSnapshot snapshot, IEnumerable<DomainEvent<TKey>> events)
			where TSnapshot : Snapshot<TKey>
		{
			if (snapshot != null)
			{
				this.AsDynamic().ApplySnapshot(snapshot);
				Stamp = snapshot.Stamp;
				Sequence = snapshot.Sequence;
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

			try
			{
				this.AsDynamic().Handle(@event);
				_events.Add(@event);
			}
			catch (MissingMethodException)
			{
				throw new MissingMethodException(GetType().Name, $"Handle({@eventType.Name} e)");
			}
		}
	}
}
