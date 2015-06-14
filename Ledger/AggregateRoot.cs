﻿using System.Collections.Generic;
using System.Linq;
using Ledger.Infrastructure;

namespace Ledger
{
	public class AggregateRoot<TKey>
	{
		public TKey ID { get; protected set; }
		public int SequenceID { get; protected set; }

		private readonly List<DomainEvent<TKey>> _events;
 
		protected AggregateRoot()
		{
			_events = new List<DomainEvent<TKey>>();
		}

		public IEnumerable<DomainEvent<TKey>> GetUncommittedEvents()
		{
			return _events;
		}

		public void MarkEventsCommitted()
		{
			if (_events.Any())
			{
				SequenceID = _events.Last().SequenceID;
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
				SequenceID = last.SequenceID;
			}
		}

		public void LoadFromSnapshot<TSnapshot>(TSnapshot snapshot, IEnumerable<DomainEvent<TKey>> events)
			where TSnapshot : ISnapshot
		{
			this.AsDynamic().ApplySnapshot(snapshot);
			SequenceID = snapshot.SequenceID;

			LoadFromEvents(events);
		}

		protected void ApplyEvent(DomainEvent<TKey> @event)
		{
			this.AsDynamic().Handle(@event);
			_events.Add(@event);
		}
	}
}
