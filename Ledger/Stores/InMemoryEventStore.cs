using System;
using System.Collections.Generic;
using System.Linq;
using Ledger.Infrastructure;

namespace Ledger.Stores
{
	public class InMemoryEventStore : IEventStore
	{
		private readonly LightweightCache<object, List<object>> _events;
		private readonly LightweightCache<object, List<object>> _snapshots;

		public InMemoryEventStore()
		{
			_events = new LightweightCache<object, List<object>>(
				key => new List<object>());

			_snapshots = new LightweightCache<object, List<object>>(
				key => new List<object>());
		}

		public IEnumerable<object> AllEvents => _events.SelectMany(events => events).OrderBy(e => ((IStamped)e).Stamp);
		public IEnumerable<object> AllSnapshots => _snapshots.SelectMany(events => events).OrderBy(e => ((IStamped)e).Stamp);

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
			private readonly LightweightCache<object, List<object>> _events;
			private readonly LightweightCache<object, List<object>> _snapshots;

			public ReaderWriter(LightweightCache<object, List<object>> events, LightweightCache<object, List<object>> snapshots)
			{
				_events = events;
				_snapshots = snapshots;
			}

			public IEnumerable<IDomainEvent<TKey>> LoadEvents(TKey aggregateID)
			{
				List<object> events;

				return _events.TryGetValue(aggregateID, out events)
					? events.Cast<IDomainEvent<TKey>>()
					: Enumerable.Empty<IDomainEvent<TKey>>();
			}

			public IEnumerable<IDomainEvent<TKey>> LoadEventsSince(TKey aggregateID, DateTime stamp)
			{
				return LoadEvents(aggregateID)
					.Where(e => e.Stamp > stamp);
			}

			public ISnapshot<TKey> LoadLatestSnapshotFor(TKey aggregateID)
			{
				List<object> snapshots;

				return _snapshots.TryGetValue(aggregateID, out snapshots)
					? snapshots.Cast<ISnapshot<TKey>>().LastOrDefault()
					: null;
			}

			public IEnumerable<TKey> LoadAllKeys()
			{
				return _events.Keys.Cast<TKey>();
			}

			public DateTime? GetLatestStampFor(TKey aggregateID)
			{
				var last = LoadEvents(aggregateID).LastOrDefault();

				return last != null
					? last.Stamp
					: (DateTime?)null;
			}

			public int GetNumberOfEventsSinceSnapshotFor(TKey aggregateID)
			{
				var last = LoadLatestSnapshotFor(aggregateID);
				var stamp = last?.Stamp ?? DateTime.MinValue;

				return LoadEvents(aggregateID)
					.Count(e => e.Stamp >= stamp);
			}

			public void SaveEvents(IEnumerable<IDomainEvent<TKey>> changes)
			{
				changes.ForEach(change => _events[change.AggregateID].Add(change));
			}

			public void SaveSnapshot(ISnapshot<TKey> snapshot)
			{
				_snapshots[snapshot.AggregateID].Add(snapshot);
			}

			public void Dispose()
			{
			}
		}
	}
}
