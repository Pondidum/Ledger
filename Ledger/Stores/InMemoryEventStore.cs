using System;
using System.Collections.Generic;
using System.Linq;
using Ledger.Infrastructure;

namespace Ledger.Stores
{
	public class InMemoryEventStore : IEventStore
	{
		private readonly LightweightCache<object, List<StampedEvent>> _events;
		private readonly LightweightCache<object, List<StampedSnapshot>> _snapshots;

		public InMemoryEventStore()
		{
			_events = new LightweightCache<object, List<StampedEvent>>(
				key => new List<StampedEvent>());

			_snapshots = new LightweightCache<object, List<StampedSnapshot>>(
				key => new List<StampedSnapshot>());
		}

		public IEnumerable<object> AllEvents => _events.SelectMany(events => events).OrderBy(e => e.GlobalSequence).Select(e => e.Event);
		public IEnumerable<object> AllSnapshots => _snapshots.SelectMany(events => events).OrderBy(e => e.GlobalSequence).Select(e => e.Snapshot);

		public IStoreReader<TKey> CreateReader<TKey>(string stream)
		{
			return new ReaderWriter<TKey>(_events, _snapshots);
		}

		public IStoreWriter<TKey> CreateWriter<TKey>(string stream)
		{
			return new ReaderWriter<TKey>(_events, _snapshots);
		}


		private class ReaderWriter<TKey> : IStoreReader<TKey>, IStoreWriter<TKey>
		{
			private readonly LightweightCache<object, List<StampedEvent>> _events;
			private readonly LightweightCache<object, List<StampedSnapshot>> _snapshots;

			public ReaderWriter(LightweightCache<object, List<StampedEvent>> events, LightweightCache<object, List<StampedSnapshot>> snapshots)
			{
				_events = events;
				_snapshots = snapshots;
			}

			public IEnumerable<IDomainEvent<TKey>> LoadEvents(TKey aggregateID)
			{
				List<StampedEvent> events;

				return _events.TryGetValue(aggregateID, out events)
					? events.Select(s => s.Event).Cast<IDomainEvent<TKey>>()
					: Enumerable.Empty<IDomainEvent<TKey>>();
			}

			public IEnumerable<IDomainEvent<TKey>> LoadEventsSince(TKey aggregateID, int sequenceID)
			{
				return LoadEvents(aggregateID)
					.Where(e => e.Sequence > sequenceID);
			}

			public ISnapshot<TKey> LoadLatestSnapshotFor(TKey aggregateID)
			{
				List<StampedSnapshot> snapshots;

				return _snapshots.TryGetValue(aggregateID, out snapshots)
					? snapshots.Select(s => s.Snapshot).Cast<ISnapshot<TKey>>().LastOrDefault()
					: null;
			}

			public IEnumerable<TKey> LoadAllKeys()
			{
				return _events.Keys.Cast<TKey>();
			}

			public int? GetLatestSequenceFor(TKey aggregateID)
			{
				var last = LoadEvents(aggregateID).LastOrDefault();

				return last != null
					? last.Sequence
					: (int?)null;
			}

			public int? GetLatestSnapshotSequenceFor(TKey aggregateID)
			{
				var last = LoadLatestSnapshotFor(aggregateID);

				return last != null
					? last.Sequence
					: (int?)null;
			}

			public void SaveEvents(IEnumerable<IDomainEvent<TKey>> changes)
			{
				changes.ForEach(change => _events[change.AggregateID].Add(new StampedEvent(change)));
			}

			public void SaveSnapshot(ISnapshot<TKey> snapshot)
			{
				_snapshots[snapshot.AggregateID].Add(new StampedSnapshot(snapshot));
			}

			public void Dispose()
			{
			}
		}

		private struct StampedEvent
		{
			private static int _globalSequence;

			public int GlobalSequence { get; }
			public object Event { get; }

			public StampedEvent(object @event)
			{
				GlobalSequence = _globalSequence++;
				Event = @event;
			}
		}

		private struct StampedSnapshot
		{
			private static int _globalSequence;

			public int GlobalSequence { get; }
			public object Snapshot { get; }

			public StampedSnapshot(object snapshot)
			{
				GlobalSequence = _globalSequence++;
				Snapshot = snapshot;
			}
		}
	}
}
