using System;
using System.Collections.Generic;
using System.Linq;

namespace Ledger.Stores
{
	public class InMemoryEventStore<TKey> : IEventStore<TKey>
	{
		private readonly Dictionary<object, List<StampedEvent>> _events;
		private readonly Dictionary<object, List<ISequenced>> _snapshots;

		private int _eventSequence;

		public InMemoryEventStore()
		{
			_events = new Dictionary<object, List<StampedEvent>>();
			_snapshots = new Dictionary<object, List<ISequenced>>();

			_eventSequence = 0;
		}

		public IEnumerable<IDomainEvent> AllEvents => _events.SelectMany(e => e.Value).OrderBy(e => e.GlobalSequence).Select(e => e.Event);

		public int? GetLatestSequenceFor(IStoreConventions storeConventions, TKey aggregateID)
		{
			var last = LoadEvents(storeConventions, aggregateID).LastOrDefault();

			return last != null
				? last.Sequence
				: (int?)null;
		}

		public int? GetLatestSnapshotSequenceFor(IStoreConventions storeConventions, TKey aggregateID)
		{
			var last = LoadLatestSnapshotFor(storeConventions, aggregateID);

			return last != null
				? last.Sequence
				: (int?)null;
		}

		public void SaveEvents(IStoreConventions storeConventions, TKey aggregateID, IEnumerable<IDomainEvent> changes)
		{
			if (_events.ContainsKey(aggregateID) == false)
			{
				_events[aggregateID] = new List<StampedEvent>();
			}

			_events[aggregateID].AddRange(changes.Select(c => new StampedEvent(c, _eventSequence++)));
		}

		public IEnumerable<IDomainEvent> LoadEvents(IStoreConventions storeConventions, TKey aggregateID)
		{
			List<StampedEvent> events;

			return _events.TryGetValue(aggregateID, out events)
				? events.Select(s => s.Event)
				: Enumerable.Empty<IDomainEvent>();
		}

		public IEnumerable<IDomainEvent> LoadEventsSince(IStoreConventions storeConventions, TKey aggregateID, int sequenceID)
		{
			return LoadEvents(storeConventions, aggregateID)
				.Where(e => e.Sequence > sequenceID);
		}

		public ISequenced LoadLatestSnapshotFor(IStoreConventions storeConventions, TKey aggregateID)
		{
			List<ISequenced> snapshots;

			return _snapshots.TryGetValue(aggregateID, out snapshots)
				? snapshots.LastOrDefault()
				: null;
		}

		public void SaveSnapshot(IStoreConventions storeConventions, TKey aggregateID, ISequenced snapshot)
		{
			if (_snapshots.ContainsKey(aggregateID) == false)
			{
				_snapshots[aggregateID] = new List<ISequenced>();
			}

			_snapshots[aggregateID].Add(snapshot);
		}

		public IEventStore<TKey> BeginTransaction()
		{
			return this;
		}

		public void Dispose()
		{
		}


		private struct StampedEvent
		{
			public int GlobalSequence { get; }
			public IDomainEvent Event { get; }

			public StampedEvent(IDomainEvent @event, int sequence)
			{
				GlobalSequence = sequence;
				Event = @event;
			}
		}
	}
}