using System;
using System.Linq;
using Ledger.Stores;
using Ledger.Tests.AggregateStoreTests.MiniDomain;
using Ledger.Tests.AggregateStoreTests.MiniDomain.Events;
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
    }
}
