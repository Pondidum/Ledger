using System;
using Ledger.Infrastructure;

namespace Ledger.Projections
{
	public class Projection : IProjectionist
	{
		public void Apply<TKey>(DomainEvent<TKey> domainEvent)
		{
			try
			{
				this.AsDynamic().Handle(domainEvent);
			}
			catch (MissingMethodException)
			{
				//do nothing, as this is a projection not an aggregate
			}
		}
	}
}
