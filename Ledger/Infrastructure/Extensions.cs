using System;
using System.Collections.Generic;
using System.Linq;

namespace Ledger.Infrastructure
{
	public static class Extensions
	{
		public static IEnumerable<T> Apply<T>(this IEnumerable<T> self, Action<T> action)
		{
			foreach (var item in self)
			{
				action(item);
				yield return item;
			}
		}

		public static IEnumerable<T> Apply<T>(this IEnumerable<T> self, Action<T, int> action)
		{
			var i = 0;
			foreach (var item in self)
			{
				action(item, i++);
				yield return item;
			}
		}

		public static void ForEach<T>(this IEnumerable<T> self, Action<T> action)
		{
			foreach (var item in self)
			{
				action(item);
			}
		}

		public static bool None<T>(this IEnumerable<T> self)
		{
			return self.Any() == false;
		}
	}
}
