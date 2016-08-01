using System;
using RabbitMQ.Client;
using Xunit;

namespace Ledger.Tests.TestInfrastructure
{
	public class RequiresRabbitFactAttribute : FactAttribute
	{
		public RequiresRabbitFactAttribute()
		{
			if (IsRabbitAvailable.Value == false)
			{
				Skip = "Postgres is not available";
			}
		}

		private static readonly Lazy<bool> IsRabbitAvailable;

		static RequiresRabbitFactAttribute()
		{
			IsRabbitAvailable = new Lazy<bool>(() =>
			{
				try
				{
					var factory = new ConnectionFactory
					{
						HostName = "10.0.75.2",
						RequestedConnectionTimeout = 1000
					};
					
					using (var connection = factory.CreateConnection())
					{
						return connection.IsOpen;
					}
				}
				catch (Exception)
				{
					return false;
				}
			});
		}

	}
}
