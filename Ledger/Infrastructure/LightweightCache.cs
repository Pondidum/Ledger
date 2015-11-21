using System;
using System.Collections;
using System.Collections.Generic;

//https://github.com/structuremap/structuremap/blob/master/src/StructureMap/Util/LightweightCache.cs
namespace Ledger.Infrastructure
{
	internal class LightweightCache<TKey, TValue> : IEnumerable<TValue> where TValue : class
	{
		private readonly IDictionary<TKey, TValue> _values;
		private readonly Func<TKey, TValue> _onMissing;

		public LightweightCache(Func<TKey, TValue> onMissing)
			: this(new Dictionary<TKey, TValue>(), onMissing)
		{
		}

		private LightweightCache(IDictionary<TKey, TValue> dictionary, Func<TKey, TValue> onMissing)
			: this(dictionary)
		{
			_onMissing = onMissing;
		}

		private LightweightCache(IDictionary<TKey, TValue> dictionary)
		{
			_values = dictionary;
		}

		public TValue this[TKey key]
		{
			get
			{
				TValue value;

				if (!_values.TryGetValue(key, out value))
				{
					value = _onMissing(key);

					if (value != null)
					{
						_values[key] = value;
					}
				}

				return value;
			}
			set
			{
				if (_values.ContainsKey(key))
				{
					_values[key] = value;
				}
				else
				{
					_values.Add(key, value);
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<TValue>) this).GetEnumerator();
		}

		public IEnumerator<TValue> GetEnumerator()
		{
			return _values.Values.GetEnumerator();
		}

		public IEnumerable<KeyValuePair<TKey, TValue>> Dictionary
		{
			get { return _values; }
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			return _values.TryGetValue(key, out value);
		}
	}
}
