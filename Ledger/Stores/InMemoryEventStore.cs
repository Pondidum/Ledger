﻿using System;
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

		public IEnumerable<object> AllEvents => _events.SelectMany(events => events).OrderBy(e => e.StreamSequence).Select(e => e.Content);
		public IEnumerable<object> AllSnapshots => _snapshots.SelectMany(events => events).OrderBy(e => e.StreamSequence).Select(e => e.Content);

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
			public StreamSequence StreamSequence { get; set; }
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

			public IEnumerable<DomainEvent<TKey>> LoadEventsSince(TKey aggregateID, Sequence? sequence)
			{
				var events = LoadEvents(aggregateID);

				return sequence.HasValue
					? events.Where(e => e.Sequence > sequence)
					: events;
			}

			public Snapshot<TKey> LoadLatestSnapshotFor(TKey aggregateID)
			{
				List<Dto> snapshots;

				return _snapshots.TryGetValue(aggregateID, out snapshots)
					? snapshots.Select(s => s.Content).Cast<Snapshot<TKey>>().LastOrDefault()
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

			public IEnumerable<DomainEvent<TKey>> LoadAllEventsSince(StreamSequence streamSequence)
			{
				return LoadAllEvents()
					.Where(e => e.StreamSequence > streamSequence);
			}

			public Sequence? GetLatestSequenceFor(TKey aggregateID)
			{
				var last = LoadEvents(aggregateID).LastOrDefault();

				return last != null
					? last.Sequence
					: (Sequence?)null;
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
				var total = _events.Sum(e => e.Count);

				changes.ForEach((change, i) =>
				{
					change.StreamSequence = new StreamSequence(total + i);

					var dto = new Dto
					{
						StreamSequence = change.StreamSequence,
						Content = change
					};

					_events[change.AggregateID].Add(dto);
				});
			}

			public void SaveSnapshot(Snapshot<TKey> snapshot)
			{
				var total = _snapshots.Sum(s => s.Count);

				_snapshots[snapshot.AggregateID].Add(new Dto
				{
					StreamSequence = new StreamSequence(total),
					Content = snapshot
				});
			}

			public void Dispose()
			{
			}
		}
	}
}
