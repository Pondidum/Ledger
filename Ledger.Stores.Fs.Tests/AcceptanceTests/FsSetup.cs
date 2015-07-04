using System;
using System.IO;

namespace Ledger.Stores.Fs.Tests.AcceptanceTests
{
	internal class FsSetup
	{
		private string _root;

		public FileEventStore<Guid> Build()
		{
			_root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

			Directory.CreateDirectory(_root);
			return new FileEventStore<Guid>(_root);
		}

		public void Clean()
		{
			try
			{
				Directory.Delete(_root, true);
			}
			catch (Exception)
			{
			}
		}
	}
}
