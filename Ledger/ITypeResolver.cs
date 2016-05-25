using System;

namespace Ledger
{
	public interface ITypeResolver
	{
		Type GetType(string typeName);
	}
}
