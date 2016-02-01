using System;
using System.Collections.Generic;

namespace Ledger
{
	public class AggregateLoadAllConfiguration<TKey>
	{
		private readonly Dictionary<Type, Func<AggregateRoot<TKey>>> _bySnapshot;
		private readonly Dictionary<Type, Func<AggregateRoot<TKey>>> _byEvent;
		private Func<ISnapshot<TKey>, DomainEvent<TKey>, AggregateRoot<TKey>> _onUnconstructable;

		public AggregateLoadAllConfiguration()
		{
			_bySnapshot = new Dictionary<Type, Func<AggregateRoot<TKey>>>();
			_byEvent = new Dictionary<Type, Func<AggregateRoot<TKey>>>();

			_onUnconstructable = (snapshot, firstEvent) =>
			{
				if (snapshot != null && firstEvent != null)
					throw new AggregateConstructionException($"No Aggregate has been defined for creation by {snapshot.GetType().Name} or {firstEvent.GetType().Name}");

				if (snapshot != null)
					throw new AggregateConstructionException($"No Aggregate has been defined for creation by {snapshot.GetType().Name}");

				if (firstEvent != null)
					throw new AggregateConstructionException($"No Aggregate has been defined for creation by {firstEvent.GetType().Name}");

				throw new AggregateConstructionException();
			};
		}

		public void Snapshot<TSnapshot>(Func<AggregateRoot<TKey>> create)
			where TSnapshot : ISnapshot<TKey>
		{
			_bySnapshot.Add(typeof(TSnapshot), create);
		}

		public void Event<TEvent>(Func<AggregateRoot<TKey>> create)
			where TEvent : DomainEvent<TKey>
		{
			_byEvent.Add(typeof(TEvent), create);
		}

		public void UnconstructableAggregate(Func<ISnapshot<TKey>, DomainEvent<TKey>, AggregateRoot<TKey>> action)
		{
			_onUnconstructable = action;
		}

		public Func<AggregateRoot<TKey>> For(ISnapshot<TKey> snapshot, DomainEvent<TKey> firstEvent)
		{
			if (snapshot != null && _bySnapshot.ContainsKey(snapshot.GetType()))
				return _bySnapshot[snapshot.GetType()];

			if (firstEvent != null && _byEvent.ContainsKey(firstEvent.GetType()))
				return _byEvent[firstEvent.GetType()];

			return () => _onUnconstructable(snapshot, firstEvent);
		}
	}
}
