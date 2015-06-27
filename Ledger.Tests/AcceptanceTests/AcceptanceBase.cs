﻿using Ledger.Tests.TestObjects;

namespace Ledger.Tests.AcceptanceTests
{
	public class AcceptanceBase<TAggregate>
	{
		public TAggregate Aggregate { get; set; }
		public IEventStore EventStore { get; set; }

		public AcceptanceBase()
		{
			EventStore = new FakeEventStore();
		}

		//comment out the ctor above and uncomment this to run
		//against the file system rather than in memory. Useful
		//to check a new IEventStore implementation works properly.


		//private readonly string _root;

		//public AcceptanceBase()
		//{
		//	_root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		//	Directory.CreateDirectory(_root);

		//	EventStore = new FileEventStore(_root);
		//}

		//public void Dispose()
		//{
		//	try
		//	{
		//		Directory.Delete(_root, true);
		//	}
		//	catch (Exception)
		//	{
		//	}
		//}
	}
}
