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

		private class With : AggregateRoot<Guid>, ISnapshotable<Snap>
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

		private class Without : AggregateRoot<Guid>
		{

		}

		internal class Snap : ISequenced
		{
			public int Sequence { get; set; }
		}
	}
}
