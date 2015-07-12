using System;
using System.IO;
using Ledger.Acceptance.AcceptanceTests;

namespace Ledger.Stores.Fs.Tests.AcceptanceTests
{
	public class FsLoadingMultipleEventsWithSnapshotting : LoadingMultipleEventsWithSnapshotting, IDisposable
	{
		private FileEventStore<Guid> _store;

		protected override IEventStore<Guid> EventStore
		{
			get
			{
				if (_store == null)
				{
					var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

					Directory.CreateDirectory(root);
					_store = new FileEventStore<Guid>(root);
				}

				return _store;
			}
		}

		public void Dispose()
		{
			try
			{
				Directory.Delete(_store.Directory, true);
			}
			catch (Exception)
			{
			}
		}
	}
}