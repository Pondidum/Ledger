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
					on.Event<PermissionCreatedEvent>(() => new Permission());
				})
				.Cast<Permission>()
				.Single();

			loaded.ShouldSatisfyAllConditions(
				() => loaded.ID.ShouldBe(input.ID),
				() => loaded.Name.ShouldBe("Updated")
			);
		}

		[Fact]
		public void When_loading_an_aggregate_with_a_snaphot()
		{
			var input = User.Create();
			_store.Save(StreamName, input);

			var loaded = _store
				.LoadAll(StreamName, on =>
				{
					on.Snapshot<UserSnapshot>(() => new User());
				})
				.Cast<User>()
				.Single();

			loaded.ShouldSatisfyAllConditions(
				() => loaded.ID.ShouldBe(input.ID),
				() => loaded.Name.ShouldBe("Dave")
			);
		}

		[Fact]
		public void When_loading_and_an_aggregate_has_not_had_creation_defined()
		{
			var input = Permission.Create();
			input.ChangeName("Some Permission");
			_store.Save(StreamName, input);

			Should.Throw<AggregateConstructionException>(
				() => _store.LoadAll(StreamName, on => { }).ToList());
		}

		[Fact]
		public void When_loading_and_an_unknown_aggregate_and_custom_action_is_defined()
		{
			var input = Permission.Create();
			input.ChangeName("Some Permission");
			_store.Save(StreamName, input);

			var unknownSeen = false;
			var onUnknown = new Func<ISnapshot<Guid>, IDomainEvent<Guid>, AggregateRoot<Guid>>((s, e) =>
			{
				unknownSeen = true;
				return null;
			});

			var all = _store.LoadAll(StreamName, on => { on.UnconstructableAggregate(onUnknown); }).ToList();

			all.ShouldBeEmpty();
			unknownSeen.ShouldBe(true);
		}


		private class Permission : AggregateRoot<Guid>
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
				ApplyEvent(new ChangePermissionNameEvent { NewName = newName });
			}

			private void Handle(PermissionCreatedEvent e)
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
				role.ApplyEvent(new RoleCreatedEvent { AggregateID = Guid.NewGuid() });
				return role;
			}

			private void Handle(RoleCreatedEvent e)
			{
				ID = e.AggregateID;
			}
		}

		private class User : AggregateRoot<Guid>, ISnapshotable<Guid, UserSnapshot>, ISnapshotControl
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

		private class PermissionCreatedEvent : DomainEvent<Guid> { }
		private class RoleCreatedEvent : DomainEvent<Guid> { }
		private class UserCreatedEvent : DomainEvent<Guid> { }

		private class ChangePermissionNameEvent : DomainEvent<Guid>
		{
			public string NewName { get; set; }
		}

		private class UserSnapshot : ISnapshot<Guid>
		{
			public int Sequence { get; set; }
			public Guid AggregateID { get; set; }

			public string Name { get; set; }
		}
    }
}
