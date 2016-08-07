using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ledger.Infrastructure;
using Shouldly;
using Xunit;

namespace Ledger.Tests
{
	public class DynamicInvocationTests
	{
		private readonly TestTarget _target = new TestTarget();

		[Fact]
		public void When_invoking_a_parent_class()
		{
			_target.Handle(new Parent());
			_target.Invoked.ShouldBe("parent");
		}

		[Fact]
		public void When_invoking_a_child_class()
		{
			_target.Handle(new Child());
			_target.Invoked.ShouldBe("child");
		}

		[Theory]
		[MemberData("Targets")]
		public void When_invoking_as_cast_to_object(object arg, string expected)
		{
			_target.Handle(arg);
			_target.Invoked.ShouldBe(expected);
		}

		[Fact]
		public void When_there_is_no_handler()
		{
			Should.Throw<MissingMethodException>(() => _target.Handle(new Unhandled()));
		}

		public static IEnumerable<object[]> Targets
		{
			get
			{
				yield return new object[] { new Parent(), "parent" };
				yield return new object[] { new Child(), "child" };
				yield return new object[] { new GrandChild(), "grandchild" };
				yield return new object[] { new OtherChild(), "parent" };
			}
		}

		public class TestTarget
		{
			public string Invoked { get; private set; }

			private void Handle(Parent e)
			{
				Invoked = "parent";
			}

			private void Handle(Child e)
			{
				Invoked = "child";
			}

			private void Handle(GrandChild e)
			{
				Invoked = "grandchild";
			}
		}

		public class Parent { }
		public class Child : Parent { }
		public class GrandChild : Child { }
		public class OtherChild : Parent { }
		public class Unhandled { }
	}
}
