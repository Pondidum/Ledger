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

		private int _eventSequence;
		private int _snapshotSequence;

		public InMemoryEventStore()
		{
			_events = new LightweightCache<object, List<StampedEvent>>(
				key => new List<StampedEvent>());

			_snapshots = new LightweightCache<object, List<StampedSnapshot>>(
				key => new List<StampedSnapshot>());

			_eventSequence = 0;
			_snapshotSequence = 0;
		}

		public IEnumerable<object> AllEvents => _events.SelectMany(events => events).OrderBy(e => e.GlobalSequence).Select(e => e.Event);
		public IEnumerable<object> AllSnapshots => _snapshots.SelectMany(events => events).OrderBy(e => e.GlobalSequence).Select(e => e.Snapshot);

		public IStoreReader<TKey> CreateReader<TKey>(IStoreConventions storeConventions)
		{
			return new ReaderWriter<TKey>(_events, _snapshots, ref _eventSequence, ref _snapshotSequence);
		}

		public IStoreWriter<TKey> CreateWriter<TKey>(IStoreConventions storeConventions)
		{
			return new ReaderWriter<TKey>(_events, _snapshots, ref _eventSequence, ref _snapshotSequence);
		}


		private class ReaderWriter<TKey> : IStoreReader<TKey>, IStoreWriter<TKey>
		{
			private readonly LightweightCache<object, List<StampedEvent>> _events;
			private readonly LightweightCache<object, List<StampedSnapshot>> _snapshots;
			private int _eventSequence;
			private int _snapshotSequence;

			public ReaderWriter(LightweightCache<object, List<StampedEvent>> events, LightweightCache<object, List<StampedSnapshot>> snapshots, ref int eventSequence, ref int snapshotSequence)
			{
				_events = events;
				_snapshots = snapshots;
				_eventSequence = eventSequence;
				_snapshotSequence = snapshotSequence;
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
				changes.ForEach(change => _events[change.AggregateID].Add(new StampedEvent(change, _eventSequence++)));
			}

			public void SaveSnapshot(ISnapshot<TKey> snapshot)
			{
				_snapshots[snapshot.AggregateID].Add(new StampedSnapshot(snapshot, _snapshotSequence++));
			}

			public void Dispose()
			{
			}
		}

		private struct StampedEvent
		{
			public int GlobalSequence { get; }
			public object Event { get; }

			public StampedEvent(object @event, int sequence)
			{
				GlobalSequence = sequence;
				Event = @event;
			}
		}

		private struct StampedSnapshot
		{
			public int GlobalSequence { get; }
			public object Snapshot { get; }

			public StampedSnapshot(object snapshot, int sequence)
			{
				GlobalSequence = sequence;
				Snapshot = snapshot;
			}
		}
	}
}
