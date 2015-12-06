using System;
using System.Diagnostics;
using Microsoft.Win32;

namespace Ledger
{
	public class DefaultStamper
	{
		private static readonly Stopwatch Stopwatch;
		private static DateTime _start;

		static DefaultStamper()
		{
			Stopwatch = new Stopwatch();
			Stopwatch.Start();

			_start = DateTime.UtcNow;


			SystemEvents.TimeChanged += (s, e) =>
			{
				Stopwatch.Restart();
				_start = DateTime.UtcNow;
			};
		}

		public static DateTime Now()
		{
			return _start.Add(Stopwatch.Elapsed);
		}
	}
}
