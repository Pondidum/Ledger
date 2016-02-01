using System;
using System.Collections.Generic;
using System.Linq;
using Ledger.Infrastructure;

namespace Ledger.Stores
{
	public class InMemoryEventStore : IEventStore
	{
		private readonly LightweightCache<object, List<Dto>> _events;
		private readonly LightweightCache<object, List<Dto>> _snapshots;

		public InMemoryEventStore()
		{
			_events = new LightweightCache<object, List<Dto>>(
				key => new List<Dto>());

			_snapshots = new LightweightCache<object, List<Dto>>(
				key => new List<Dto>());
		}

		public IEnumerable<object> AllEvents => _events.SelectMany(events => events).OrderBy(e => e.Stamp).Select(e => e.Content);
		public IEnumerable<object> AllSnapshots => _snapshots.SelectMany(events => events).OrderBy(e => e.Stamp).Select(e => e.Content);

		public IStoreReader<TKey> CreateReader<TKey>(EventStoreContext context)
		{
			return new ReaderWriter<TKey>(_events, _snapshots);
		}

		public IStoreWriter<TKey> CreateWriter<TKey>(EventStoreContext context)
		{
			return new ReaderWriter<TKey>(_events, _snapshots);
		}

		private struct Dto
		{
			public DateTime Stamp { get; set; }
			public object Content { get; set; }
		}

		private class ReaderWriter<TKey> : IStoreReader<TKey>, IStoreWriter<TKey>
		{
			private readonly LightweightCache<object, List<Dto>> _events;
			private readonly LightweightCache<object, List<Dto>> _snapshots;

			public ReaderWriter(LightweightCache<object, List<Dto>> events, LightweightCache<object, List<Dto>> snapshots)
			{
				_events = events;
				_snapshots = snapshots;
			}

			public IEnumerable<DomainEvent<TKey>> LoadEvents(TKey aggregateID)
			{
				List<Dto> events;

				return _events.TryGetValue(aggregateID, out events)
					? events.Select(e => e.Content).Cast<DomainEvent<TKey>>()
					: Enumerable.Empty<DomainEvent<TKey>>();
			}

			public IEnumerable<DomainEvent<TKey>> LoadEventsSince(TKey aggregateID, DateTime? stamp)
			{
				var events = LoadEvents(aggregateID);

				return stamp.HasValue
					? events.Where(e => e.Stamp > stamp)
					: events;
			}

			public ISnapshot<TKey> LoadLatestSnapshotFor(TKey aggregateID)
			{
				List<Dto> snapshots;

				return _snapshots.TryGetValue(aggregateID, out snapshots)
					? snapshots.Select(s => s.Content).Cast<ISnapshot<TKey>>().LastOrDefault()
					: null;
			}

			public IEnumerable<TKey> LoadAllKeys()
			{
				return _events.Keys.Cast<TKey>()
					.Concat(_snapshots.Keys.Cast<TKey>())
					.Distinct();
			}

			public IEnumerable<DomainEvent<TKey>> LoadAllEvents()
			{
				return _events
					.SelectMany(e => e)
					.Select(e => e.Content)
					.Cast<DomainEvent<TKey>>()
					.OrderBy(e => e.Stamp);
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

			public void SaveEvents(IEnumerable<DomainEvent<TKey>> changes)
			{
				changes.ForEach(change => _events[change.AggregateID].Add(new Dto { Stamp = change.Stamp, Content = change }));
			}

			public void SaveSnapshot(ISnapshot<TKey> snapshot)
			{
				_snapshots[snapshot.AggregateID].Add(new Dto { Stamp = snapshot.Stamp, Content = snapshot });
			}

			public void Dispose()
			{
			}
		}
	}
}
