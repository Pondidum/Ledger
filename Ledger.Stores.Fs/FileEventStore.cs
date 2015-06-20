using System.Collections.Generic;
using System.IO;
using Ledger.Infrastructure;
using Newtonsoft.Json;

namespace Ledger.Stores.Fs
{
	public class FileEventStore : IEventStore
	{
		private readonly string _directory;

		public FileEventStore(string directory)
		{
			_directory = directory;
		}

		public void SaveEvents<TKey>(TKey aggegateID, IEnumerable<DomainEvent> changes)
		{
			var eventFile = Path.Combine(_directory, typeof(TKey).Name + ".events.json");

			using (var fs = new FileStream(eventFile, FileMode.Append))
			using (var sw = new StreamWriter(fs))
			{
				changes.ForEach(change =>
				{
					var dto = new EventDto<TKey> { ID = aggegateID, Event = change };
					var json = JsonConvert.SerializeObject(dto);

					sw.WriteLine(json);
				});
			}
		}

		public int? GetLatestSequenceIDFor<TKey>(TKey aggegateID)
		{
			throw new System.NotImplementedException();
		}

		public IEnumerable<DomainEvent> LoadEvents<TKey>(TKey aggegateID)
		{
			throw new System.NotImplementedException();
		}

		public IEnumerable<DomainEvent> LoadEventsSince<TKey>(TKey aggegateID, int sequenceID)
		{
			throw new System.NotImplementedException();
		}

		public ISequenced GetLatestSnapshotFor<TKey>(TKey aggegateID)
		{
			throw new System.NotImplementedException();
		}

		public void SaveSnapshot<TKey>(TKey aggregateID, ISequenced snapshot)
		{
			throw new System.NotImplementedException();
		}
	}
}
