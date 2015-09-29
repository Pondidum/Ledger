﻿using System.Collections.Generic;
using System.Linq;

namespace Ledger.Stores
{
	public class InMemoryEventStore<TKey> : IEventStore<TKey>
	{
		private readonly Dictionary<object, List<IDomainEvent>> _events;
		private readonly Dictionary<object, List<ISequenced>> _snapshots;

		public InMemoryEventStore()
		{
			_events = new Dictionary<object, List<IDomainEvent>>();
			_snapshots = new Dictionary<object, List<ISequenced>>();
		}

		public int? GetLatestSequenceFor(TKey aggregateID)
		{
			var last = LoadEvents(aggregateID).LastOrDefault();

			return last != null
				? last.Sequence
				: (int?) null;
		}

		public int? GetLatestSnapshotSequenceFor(TKey aggregateID)
		{
			var last = LoadLatestSnapshotFor(aggregateID);

			return last != null
				? last.Sequence
				: (int?) null;
		}

		public void SaveEvents(TKey aggregateID, IEnumerable<IDomainEvent> changes)
		{
			if (_events.ContainsKey(aggregateID) == false)
			{
				_events[aggregateID] = new List<IDomainEvent>();
			}

			_events[aggregateID].AddRange(changes);
		}

		public IEnumerable<IDomainEvent> LoadEvents(TKey aggregateID)
		{
			List<IDomainEvent> events;

			return _events.TryGetValue(aggregateID, out events)
				? events
				: Enumerable.Empty<IDomainEvent>();
		}

		public IEnumerable<IDomainEvent> LoadEventsSince(TKey aggregateID, int sequenceID)
		{
			return LoadEvents(aggregateID)
				.Where(e => e.Sequence > sequenceID);
		}

		public ISequenced LoadLatestSnapshotFor(TKey aggregateID)
		{
			List<ISequenced> snapshots;

			return _snapshots.TryGetValue(aggregateID, out snapshots) 
				? snapshots.LastOrDefault() 
				: null;
		}

		public void SaveSnapshot(TKey aggregateID, ISequenced snapshot)
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
	}
}