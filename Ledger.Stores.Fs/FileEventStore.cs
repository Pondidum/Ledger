using System;
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

		private string EventFile<TKey>()
		{
			return Path.Combine(_directory, typeof(TKey).Name + ".events.json");
		}

		private string SnapshotFile<TKey>()
		{
			return Path.Combine(_directory, typeof(TKey).Name + ".snapshots.json");
		}

		private void AppendTo(string filepath, Action<StreamWriter> action)
		{
			using(var fs = new FileStream(filepath, FileMode.Append))
			using (var sw = new StreamWriter(fs))
			{
				action(sw);
			}
		}

		public void SaveEvents<TKey>(TKey aggegateID, IEnumerable<DomainEvent> changes)
		{
			AppendTo(EventFile<TKey>(), file =>
			{
				changes.ForEach(change =>
				{
					var dto = new EventDto<TKey> {ID = aggegateID, Event = change};
					var json = JsonConvert.SerializeObject(dto);

					file.WriteLine(json);
				});
			});
		}

		public void SaveSnapshot<TKey>(TKey aggregateID, ISequenced snapshot)
		{
			AppendTo(SnapshotFile<TKey>(), file =>
			{
				var dto = new SnapshotDto<TKey> {ID = aggregateID, Snapshot = snapshot};
				var json = JsonConvert.SerializeObject(dto);

				file.WriteLine(json);
			});
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

	}
}
