using System;
using System.Collections.Generic;
using System.Linq;

namespace Ledger.Stores
{
	public class InMemoryEventStore : IEventStore
	{
		private readonly Dictionary<object, List<StampedEvent>> _events;
		private readonly Dictionary<object, List<StampedSnapshot>> _snapshots;

		private int _eventSequence;
		private int _snapshotSequence;

		public InMemoryEventStore()
		{
			_events = new Dictionary<object, List<StampedEvent>>();
			_snapshots = new Dictionary<object, List<StampedSnapshot>>();

			_eventSequence = 0;
			_snapshotSequence = 0;
		}

		public IEnumerable<object> AllEvents => _events.SelectMany(e => e.Value).OrderBy(e => e.GlobalSequence).Select(e => e.Event);
		public IEnumerable<object> AllSnapshots => _snapshots.SelectMany(e => e.Value).OrderBy(e => e.GlobalSequence).Select(e => e.Snapshot);

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
			private readonly Dictionary<object, List<StampedEvent>> _events;
			private readonly Dictionary<object, List<StampedSnapshot>> _snapshots;
			private int _eventSequence;
			private int _snapshotSequence;

			public ReaderWriter(Dictionary<object, List<StampedEvent>> events, Dictionary<object, List<StampedSnapshot>> snapshots, ref int eventSequence, ref int snapshotSequence)
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

			public ISnapshot LoadLatestSnapshotFor(TKey aggregateID)
			{
				List<StampedSnapshot> snapshots;

				return _snapshots.TryGetValue(aggregateID, out snapshots)
					? snapshots.Select(s => s.Snapshot).LastOrDefault()
					: null;
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

			public void SaveEvents(TKey aggregateID, IEnumerable<IDomainEvent<TKey>> changes)
			{
				if (_events.ContainsKey(aggregateID) == false)
				{
					_events[aggregateID] = new List<StampedEvent>();
				}

				_events[aggregateID].AddRange(changes.Select(c => new StampedEvent(c, _eventSequence++)));
			}

			public void SaveSnapshot(TKey aggregateID, ISnapshot snapshot)
			{
				if (_snapshots.ContainsKey(aggregateID) == false)
				{
					_snapshots[aggregateID] = new List<StampedSnapshot>();
				}

				_snapshots[aggregateID].Add(new StampedSnapshot(snapshot, _snapshotSequence++));
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
			public ISnapshot Snapshot { get; }

			public StampedSnapshot(ISnapshot snapshot, int sequence)
			{
				GlobalSequence = sequence;
				Snapshot = snapshot;
			}
		}
	}
}
