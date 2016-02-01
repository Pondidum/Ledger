﻿using System;
using System.Collections.Generic;

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

		public virtual IEnumerable<DomainEvent<TKey>> LoadEventsSince(TKey aggregateID, DateTime? stamp)
		{
			return _other.LoadEventsSince(aggregateID, stamp);
		}

		public virtual ISnapshot<TKey> LoadLatestSnapshotFor(TKey aggregateID)
		{
			return _other.LoadLatestSnapshotFor(aggregateID);
		}

		public IEnumerable<TKey> LoadAllKeys()
		{
			return _other.LoadAllKeys();
		}

		public IEnumerable<DomainEvent<TKey>> LoadAllEvents()
		{
			return _other.LoadAllEvents();
		}

		public virtual void Dispose()
		{
			_other.Dispose();
		}
	}
}
