using System;

namespace Ledger.Infrastructure
{
	public struct Sequence : IEquatable<Sequence>
	{
		public static readonly Sequence Start = new Sequence(-1);
		private readonly int _value;

		public Sequence(int value)
		{
			_value = value;
		}

		public Sequence(Sequence sequence, int offset)
			: this(sequence._value + offset)
		{
		}

		public bool Equals(Sequence other)
		{
			return _value == other._value;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Sequence && Equals((Sequence)obj);
		}

		public override int GetHashCode()
		{
			return _value;
		}

		public override string ToString()
		{
			return _value.ToString();
		}

		public static bool operator ==(Sequence s1, Sequence s2)
		{
			return s1._value == s2._value;
		}

		public static bool operator !=(Sequence s1, Sequence s2)
		{
			return s1._value != s2._value;
		}

		public static bool operator <(Sequence s1, Sequence s2)
		{
			return s1._value < s2._value;
		}

		public static bool operator >(Sequence s1, Sequence s2)
		{
			return s1._value > s2._value;
		}

		public static explicit operator int(Sequence sequence)
		{
			return sequence._value;
		}
	}
}
