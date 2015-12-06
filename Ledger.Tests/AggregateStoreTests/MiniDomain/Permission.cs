using System;
using Ledger.Tests.AggregateStoreTests.MiniDomain.Events;

namespace Ledger.Tests.AggregateStoreTests.MiniDomain
{
	public class Permission : AggregateRoot<Guid>
	{
		public string Name { get; private set; }

		public static Permission Create()
		{
			var perm = new Permission();
			perm.ApplyEvent(new PermissionCreatedEvent { AggregateID = Guid.NewGuid() });
			return perm;
		}

		public void ChangeName(string newName)
		{
			ApplyEvent(new PermissionNameChangedEvent { NewName = newName });
		}

		private void Handle(PermissionCreatedEvent e)
		{
			ID = e.AggregateID;
			Name = "New";
		}

		private void Handle(PermissionNameChangedEvent e)
		{
			Name = e.NewName;
		}

	}
}