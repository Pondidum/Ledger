using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

		private IEnumerable<TDto> ReadFrom<TDto>(string filepath)
		{
			using (var fs = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			using (var sr = new StreamReader(fs))
			{
				string line;
				while ((line = sr.ReadLine()) != null)
				{
					yield return JsonConvert.DeserializeObject<TDto>(line);
				}
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
			return LoadEvents(aggegateID)
				.Select(e => (int?) e.SequenceID)
				.Max();
		}

		public IEnumerable<DomainEvent> LoadEvents<TKey>(TKey aggegateID)
		{
			return ReadFrom<EventDto<TKey>>(EventFile<TKey>())
				.Where(dto => Equals(dto.ID, aggegateID))
				.Select(dto => dto.Event);
		}

		public IEnumerable<DomainEvent> LoadEventsSince<TKey>(TKey aggegateID, int sequenceID)
		{
			return LoadEvents(aggegateID)
				.Where(e => e.SequenceID > sequenceID);
		}

		public ISequenced GetLatestSnapshotFor<TKey>(TKey aggegateID)
		{
			return ReadFrom<SnapshotDto<TKey>>(SnapshotFile<TKey>())
				.Where(dto => Equals(dto.ID, aggegateID))
				.Select(dto => dto.Snapshot)
				.Cast<ISequenced>()
				.Last();
		}
	}
}
