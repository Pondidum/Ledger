﻿using Ledger.Infrastructure;
using Shouldly;
using Xunit;

namespace Ledger.Tests.Infrastructure
{
	public class SequenceTests
	{
		[Theory]
		[InlineData(1, 1, true)]
		[InlineData(1, 2, false)]
		[InlineData(2, 1, false)]
		[InlineData(2, 2, true)]
		[InlineData(1, null, false)]
		[InlineData(null, 1, false)]
		[InlineData(null, null, true)]
		public void When_checking_equality(int? v1, int? v2, bool same)
		{
			var s1 = v1.HasValue ? new Sequence(v1.Value) : (Sequence?)null;
			var s2 = v2.HasValue ? new Sequence(v2.Value) : (Sequence?)null;

			if (same)
				s1.ShouldBe(s2);
			else
				s1.ShouldNotBe(s2);
		}

		[Theory]
		[InlineData(1, 1, false)]
		[InlineData(1, 2, true)]
		[InlineData(2, 1, true)]
		[InlineData(2, 2, false)]
		[InlineData(1, null, true)]
		[InlineData(null, 1, true)]
		[InlineData(null, null, false)]
		public void When_checking_non_equality(int? v1, int? v2, bool same)
		{
			var s1 = v1.HasValue ? new Sequence(v1.Value) : (Sequence?)null;
			var s2 = v2.HasValue ? new Sequence(v2.Value) : (Sequence?)null;

			if (same)
				s1.ShouldNotBe(s2);
			else
				s1.ShouldBe(s2);
		}

		[Theory]
		[InlineData(1, 1, false)]
		[InlineData(1, 2, true)]
		[InlineData(2, 1, false)]
		[InlineData(2, 2, false)]
		[InlineData(1, null, false)]
		[InlineData(null, 1, false)]
		[InlineData(null, null, false)]
		public void When_checking_less_than(int? v1, int? v2, bool same)
		{
			var s1 = v1.HasValue ? new Sequence(v1.Value) : (Sequence?)null;
			var s2 = v2.HasValue ? new Sequence(v2.Value) : (Sequence?)null;

			if (same)
				(s1 < s2).ShouldBeTrue();
			else
				(s1 < s2).ShouldBeFalse();
		}

		[Theory]
		[InlineData(1, 1, false)]
		[InlineData(1, 2, false)]
		[InlineData(2, 1, true)]
		[InlineData(2, 2, false)]
		[InlineData(1, null, false)]
		[InlineData(null, 1, false)]
		[InlineData(null, null, false)]
		public void When_checking_greater_than(int? v1, int? v2, bool same)
		{
			var s1 = v1.HasValue ? new Sequence(v1.Value) : (Sequence?)null;
			var s2 = v2.HasValue ? new Sequence(v2.Value) : (Sequence?)null;

			if (same)
				(s1 > s2).ShouldBeTrue();
			else
				(s1 > s2).ShouldBeFalse();
		}

		[Fact]
		public void When_casting_to_int()
		{
			var i = (int)new Sequence(123);

			i.ShouldBe(123);
		}
	}
}
