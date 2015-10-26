using System.Collections.Generic;
using System.Linq;
using Ledger.Infrastructure;

namespace Ledger
{
	public class AggregateRoot<TKey>
	{
		public TKey ID { get; protected set; }
		public int SequenceID { get; protected set; }

		private readonly List<IDomainEvent> _events;
 
		protected AggregateRoot()
		{
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

		protected void ApplyEvent(IDomainEvent @event)
		{
			this.AsDynamic().Handle(@event);
			_events.Add(@event);
		}
	}
}
