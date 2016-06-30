using System;
using System.Collections.Generic;

namespace Ledger
{
	public class Projector<TKey>
	{
		private readonly Dictionary<Type, Action<DomainEvent<TKey>>> _projections;

		public Projector()
		{
			_projections = new Dictionary<Type, Action<DomainEvent<TKey>>>();
		}

		public IEnumerable<Type> RegisteredEvents => _projections.Keys;

		public void Register<TEvent>(Action<TEvent> projection) where TEvent : DomainEvent<TKey>
		{
			_projections[typeof(TEvent)] = e => projection((TEvent)e);
		}

		public void Apply(DomainEvent<TKey> e)
		{
			Action<DomainEvent<TKey>> projection;

			if (_projections.TryGetValue(e.GetType(), out projection))
				projection(e);
		}
	}
}
