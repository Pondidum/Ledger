using System;
using Ledger.Tests.AggregateStoreTests.MiniDomain.Events;

namespace Ledger.Tests.AggregateStoreTests.MiniDomain
{
	public class User : AggregateRoot<Guid>, ISnapshotable<Guid, UserSnapshot>, ISnapshotControl
	{
		public string Name { get; private set; }

		public static User Create()
		{
			var user = new User();
			user.ApplyEvent(new UserCreatedEvent { AggregateID = Guid.NewGuid() });
			return user;
		}

		private void Handle(UserCreatedEvent e)
		{
			ID = e.AggregateID;
		}

		public UserSnapshot CreateSnapshot()
		{
			return new UserSnapshot { Name = "Dave" };
		}

		public void ApplySnapshot(UserSnapshot snapshot)
		{
			Name = snapshot.Name;
		}

		public int SnapshotInterval
		{
			get { return 1; }
		}
	}
}