using System.Collections.Generic;
using System.Linq;
using Ledger.Infrastructure;

namespace Ledger
{
	public class AggregateRoot<TKey>
	{
		public TKey ID { get; protected set; }
		public int SequenceID { get; protected set; }

		private readonly List<DomainEvent> _events;
 
		protected AggregateRoot()
		{
			_events = new List<DomainEvent>();
		}

		public IEnumerable<DomainEvent> GetUncommittedEvents()
		{
			return _events;
		}

		internal void MarkEventsCommitted()
		{
			if (_events.Any())
			{
				SequenceID = _events.Last().SequenceID;
				_events.Clear();
			}
		}

		internal void LoadFromEvents(IEnumerable<DomainEvent> eventStream)
		{
			DomainEvent last = null;
			var dynamic = this.AsDynamic();

			eventStream
				.Apply(e => last = e)
				.ForEach(e => dynamic.Handle(e));

			if (last != null)
			{
				SequenceID = last.SequenceID;
			}
		}

		internal void LoadFromSnapshot<TSnapshot>(TSnapshot snapshot, IEnumerable<DomainEvent> events)
			where TSnapshot : ISequenced
		{
			this.AsDynamic().ApplySnapshot(snapshot);
			SequenceID = snapshot.SequenceID;

			LoadFromEvents(events);
		}

		protected void ApplyEvent(DomainEvent @event)
		{
			this.AsDynamic().Handle(@event);
			_events.Add(@event);
		}
	}
}
