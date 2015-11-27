using System;
using System.Collections.Generic;

namespace Ledger
{
	public class AggregateLoadAllConfiguration<TKey>
	{
		private readonly Dictionary<Type, Func<AggregateRoot<TKey>>> _bySnapshot;
		private readonly Dictionary<Type, Func<AggregateRoot<TKey>>> _byEvent;

		public AggregateLoadAllConfiguration()
		{
			_bySnapshot = new Dictionary<Type, Func<AggregateRoot<TKey>>>();
			_byEvent = new Dictionary<Type, Func<AggregateRoot<TKey>>>();
		}

		public void Snapshot<TSnapshot>(Func<AggregateRoot<TKey>> create)
			where TSnapshot : ISnapshot<TKey>
		{
			_bySnapshot.Add(typeof(TSnapshot), create);
		}

		public void Event<TEvent>(Func<AggregateRoot<TKey>> create)
			where TEvent : IDomainEvent<TKey>
		{
			_byEvent.Add(typeof(TEvent), create);

		}

		public Func<AggregateRoot<TKey>> For(ISnapshot<TKey> snapshot, IDomainEvent<TKey> firstEvent)
		{
			if (snapshot != null)
				return _bySnapshot[snapshot.GetType()];

			if (firstEvent != null)
				return _byEvent[firstEvent.GetType()];

			return () => { throw new NotSupportedException(); };
		}
	}
}
