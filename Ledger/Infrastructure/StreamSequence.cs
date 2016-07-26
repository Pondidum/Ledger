using System;

namespace Ledger.Infrastructure
{
	public struct StreamSequence : IEquatable<StreamSequence>, IComparable<StreamSequence>
	{
		public static readonly StreamSequence Start = new StreamSequence(-1);
		private readonly int _value;

		public StreamSequence(int value)
		{
			_value = value;
		}

		public StreamSequence(StreamSequence sequence, int offset)
			: this(sequence._value + offset)
		{
		}

		public bool Equals(StreamSequence other)
		{
			return _value == other._value;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is StreamSequence && Equals((StreamSequence)obj);
		}

		public override int GetHashCode()
		{
			return _value;
		}

		public int CompareTo(StreamSequence other)
		{
			return _value.CompareTo(other._value);
		}

		public override string ToString()
		{
			return _value.ToString();
		}

		public static bool operator ==(StreamSequence s1, StreamSequence s2)
		{
			return s1._value == s2._value;
		}

		public static bool operator !=(StreamSequence s1, StreamSequence s2)
		{
			return s1._value != s2._value;
		}

		public static bool operator <(StreamSequence s1, StreamSequence s2)
		{
			return s1._value < s2._value;
		}

		public static bool operator >(StreamSequence s1, StreamSequence s2)
		{
			return s1._value > s2._value;
		}

		public static explicit operator int(StreamSequence sequence)
		{
			return sequence._value;
		}
	}
}
