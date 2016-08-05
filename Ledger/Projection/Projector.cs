using System;
using System.Collections.Generic;
using System.Linq;
using Ledger.Infrastructure;

namespace Ledger.Projection
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
			_projections
				.Where(p => p.Key.IsInstanceOfType(e))
				.Select(p => p.Value)
				.ForEach(project => project(e));
		}
	}
}
