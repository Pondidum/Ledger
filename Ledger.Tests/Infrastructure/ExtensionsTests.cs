using System;
using Ledger.Infrastructure;
using Shouldly;
using Xunit;

namespace Ledger.Tests.Infrastructure
{
	public class ExtensionsTests
	{
		[Fact]
		public void When_the_type_implements_isnapshottable()
		{
			typeof(With)
				.ImplementsSnapshottable()
				.ShouldBe(true);
		}

		[Fact]
		public void When_the_type_doesnt_implement_isnapshottable()
		{
			typeof(Without)
				.ImplementsSnapshottable()
				.ShouldBe(false);
		}

		[Fact]
		public void When_a_parent_type_implements_isnapshottable()
		{
			typeof(SubWith)
				.ImplementsSnapshottable()
				.ShouldBe(true);
		}

		[Fact]
		public void When_a_parent_type_doesnt_implement_isnapshottable()
		{
			typeof(SubWithout)
				.ImplementsSnapshottable()
				.ShouldBe(false);
		}

		private class SubWith : With, ITest
		{
		}

		private class With : AggregateRoot<Guid>, ISnapshotable<Guid, Snap>, ITest
		{
			public Snap CreateSnapshot()
			{
				throw new NotImplementedException();
			}

			public void ApplySnapshot(Snap snapshot)
			{
				throw new NotImplementedException();
			}
		}

		private class SubWithout : Without, ITest
		{
		}

		private class Without : AggregateRoot<Guid>, ITest
		{

		}

		internal class Snap : ISnapshot<Guid>
		{
			public Guid AggregateID { get; set; }
			public DateTime Stamp { get; set; }
		}

		private interface ITest
		{ 
		}
	}
}
