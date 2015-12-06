using System;

namespace Ledger.Acceptance
{
	public class IncrementingStamper
	{
		private readonly Func<DateTime> _stamper;

		public IncrementingStamper()
		{
			Start = DateTime.UtcNow;
			var offset = 0;
			_stamper = () => Start.AddSeconds(offset++);
		}

		public DateTime Start { get; }

		public DateTime GetNext()
		{
			return _stamper();
		}

		public DateTime Offset(int offset)
		{
			return Start.AddSeconds(offset);
		}
	}
}
