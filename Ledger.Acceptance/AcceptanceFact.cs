using Xunit;

namespace Ledger.Acceptance
{
	public class AcceptanceFact : FactAttribute
	{
		private static string _skip;

		static AcceptanceFact()
		{
			_skip = "";
		}

		public static void SkipAcceptance(string reason)
		{
			_skip = reason;
		}

		public override string Skip
		{
			get { return _skip; }
			set { /* nothing */ }
		}
	}
}
