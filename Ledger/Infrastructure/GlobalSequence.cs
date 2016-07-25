using System;

namespace Ledger.Infrastructure
{
	public struct GlobalSequence : IEquatable<GlobalSequence>, IComparable<GlobalSequence>
	{
		public static readonly GlobalSequence Start = new GlobalSequence(-1);
		private readonly int _value;

		public GlobalSequence(int value)
		{
			_value = value;
		}

		public GlobalSequence(GlobalSequence sequence, int offset)
			: this(sequence._value + offset)
		{
		}

		public bool Equals(GlobalSequence other)
		{
			return _value == other._value;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is GlobalSequence && Equals((GlobalSequence)obj);
		}

		public override int GetHashCode()
		{
			return _value;
		}

		public int CompareTo(GlobalSequence other)
		{
			return _value.CompareTo(other._value);
		}

		public override string ToString()
		{
			return _value.ToString();
		}

		public static bool operator ==(GlobalSequence s1, GlobalSequence s2)
		{
			return s1._value == s2._value;
		}

		public static bool operator !=(GlobalSequence s1, GlobalSequence s2)
		{
			return s1._value != s2._value;
		}

		public static bool operator <(GlobalSequence s1, GlobalSequence s2)
		{
			return s1._value < s2._value;
		}

		public static bool operator >(GlobalSequence s1, GlobalSequence s2)
		{
			return s1._value > s2._value;
		}
	}
}
