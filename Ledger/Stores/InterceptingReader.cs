﻿using System;
using System.Collections.Generic;
using Ledger.Infrastructure;

namespace Ledger.Stores
{
	public class InterceptingReader<TKey> : IStoreReader<TKey>
	{
		private readonly IStoreReader<TKey> _other;

		public InterceptingReader(IStoreReader<TKey> other)
		{
			_other = other;
		}

		public virtual IEnumerable<DomainEvent<TKey>> LoadEvents(TKey aggregateID)
		{
			return _other.LoadEvents(aggregateID);
		}

		public virtual IEnumerable<DomainEvent<TKey>> LoadEventsSince(TKey aggregateID, Sequence? sequence)
		{
			return _other.LoadEventsSince(aggregateID, sequence);
		}

		public virtual Snapshot<TKey> LoadLatestSnapshotFor(TKey aggregateID)
		{
			return _other.LoadLatestSnapshotFor(aggregateID);
		}

		public virtual IEnumerable<TKey> LoadAllKeys()
		{
			return _other.LoadAllKeys();
		}

		public virtual IEnumerable<DomainEvent<TKey>> LoadAllEvents()
		{
			return _other.LoadAllEvents();
		}

		public virtual IEnumerable<DomainEvent<TKey>> LoadAllEventsSince(StreamSequence streamSequence)
		{
			return _other.LoadAllEventsSince(streamSequence);
		}

		public virtual void Dispose()
		{
			_other.Dispose();
		}
	}
}
