using System;
using Ledger.Projection;
using Ledger.Stores;

namespace Ledger.Tests.Examples.InProcessProjection
{
	public class BridgingProjectionist : IProjectionist
	{
		private Action<object> _handler;

		public void Project<TKey>(DomainEvent<TKey> domainEvent)
		{
			_handler(domainEvent);
		}

		public void OnEvent<TKey>(Action<DomainEvent<TKey>> handler)
		{
			_handler = e => handler((DomainEvent<TKey>)e);
		}
	}
}
