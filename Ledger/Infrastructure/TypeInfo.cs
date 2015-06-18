using System;
using System.Linq.Expressions;

namespace Ledger.Infrastructure
{
	public class TypeInfo
	{
		private static string GetMethodInfoInternal(dynamic expression)
		{
			var method = expression.Body as MethodCallExpression;
			if (method == null) throw new ArgumentException("Expression is incorrect!");

			return method.Method.Name;
		}

		public static string GetMethodName<T>(Expression<Action<T>> expression)
		{
			return GetMethodInfoInternal(expression);
		} 
	}
}