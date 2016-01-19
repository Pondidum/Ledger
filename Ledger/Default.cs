using Newtonsoft.Json;

namespace Ledger
{
	public static class Default
	{
		public static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
		{
			TypeNameHandling = TypeNameHandling.Auto
		};
	}
}
