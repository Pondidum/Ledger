using System;

namespace Ledger
{
	public class DefaultTypeResolver : ITypeResolver
	{
		public Type GetType(string typeName)
		{
			return Type.GetType(typeName);
		}
	}
}
