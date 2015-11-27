using System;
using System.Linq;
using Ledger.Stores;
using Shouldly;
using Xunit;

namespace Ledger.Tests
{
	public class LoadAllAggregatesTests
	{

		private const string StreamName = "someStream";

		private readonly InMemoryEventStore _backing;
		private readonly AggregateStore<Guid> _store;

		public LoadAllAggregatesTests()
		{
			_backing = new InMemoryEventStore();
			_store = new AggregateStore<Guid>(_backing);
		}

		[Fact]
		public void When_loading_by_events()
		{
			var input = Permission.Create();
			input.ChangeName("Updated");

			_store.Save(StreamName, input);

			var loaded = _store
				.LoadAll(StreamName, on =>
				{
					on.Event<CreatePermissionEvent>(() => new Permission());
				})
				.Cast<Permission>()
				.Single();

			loaded.ShouldSatisfyAllConditions(
				() => loaded.ID.ShouldBe(input.ID),
				() => loaded.Name.ShouldBe("Updated")
			);
		}


		private class Permission : AggregateRoot<Guid>
		{
			public string Name { get; private set; }

			public static Permission Create()
			{
				var perm = new Permission();
				perm.ApplyEvent(new CreatePermissionEvent { AggregateID = Guid.NewGuid() });
				return perm;
			}

			public void ChangeName(string newName)
			{
				ApplyEvent(new ChangePermissionNameEvent { NewName = newName });
			}

			private void Handle(CreatePermissionEvent e)
			{
				ID = e.AggregateID;
				Name = "New";
			}

			private void Handle(ChangePermissionNameEvent e)
			{
				Name = e.NewName;
			}

		}

		private class Role : AggregateRoot<Guid>
		{
			public static Role Create()
			{
				var role = new Role();
				role.ApplyEvent(new CreateRoleEvent { AggregateID = Guid.NewGuid() });
				return role;
			}

			private void Handle(CreateRoleEvent e)
			{
				ID = e.AggregateID;
			}
		}

		private class CreatePermissionEvent : DomainEvent<Guid>
		{
		}

		private class CreateRoleEvent : DomainEvent<Guid>
		{
		}

		private class ChangePermissionNameEvent : DomainEvent<Guid>
		{
			public string NewName { get; set; }
		}
	}
}
