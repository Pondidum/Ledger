using System;
using Ledger.Tests.AggregateStoreTests.MiniDomain.Events;

namespace Ledger.Tests.AggregateStoreTests.MiniDomain
{
	public class Role : AggregateRoot<Guid>
	{
		public string Name { get; private set; }

		public static Role Create()
		{
			var role = new Role();
			role.ApplyEvent(new RoleCreatedEvent { AggregateID = Guid.NewGuid() });
			return role;
		}

		public void ChangeName(string newName)
		{
			ApplyEvent(new RoleNameChangedEvent { NewName = newName });
		}

		private void Handle(RoleCreatedEvent e)
		{
			ID = e.AggregateID;
			Name = "New Role";
		}

		private void Handle(RoleNameChangedEvent e)
		{
			Name = e.NewName;
		}
	}
}