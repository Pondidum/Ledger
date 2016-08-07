using System;
using System.Linq;
using System.Reflection;

namespace Ledger.Infrastructure
{
	public static class DynamicInvocation
	{
		public static void Handle<TEvent>(this object self, TEvent domainEvent)
		{
			var allHandlers = self
				.GetType()
				.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
				.Where(method => method.Name == "Handle")
				.Where(method => method.GetParameters().Length == 1)
				.ToArray();

			var parameterType = domainEvent.GetType();

			while (parameterType != typeof(object))
			{
				var handler = allHandlers.FirstOrDefault(method => method.GetParameters().Single().ParameterType == parameterType);

				if (handler != null)
				{
					handler.Invoke(self, new object[] { domainEvent });
					return;
				}

				parameterType = parameterType.BaseType;
			}

			throw new MissingMethodException(self.GetType().Name, $"Handles({domainEvent.GetType().Name} e)");
		}
	}
}
