using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ledger.Infrastructure;
using Newtonsoft.Json;

namespace Ledger.Stores.Fs
{
	public class FileEventStore<TKey> : IEventStore<TKey>
	{
		private readonly IFileSystem _fileSystem;
		private readonly string _directory;
		private readonly JsonSerializerSettings _jsonSettings;

		public FileEventStore(string directory)
			: this(new PhysicalFileSystem(), directory)
		{
		}

		public FileEventStore(IFileSystem fs, string directory)
		{
			_fileSystem = fs;
			_directory = directory;
			_jsonSettings = new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Objects};
		}

		private string EventFile()
		{
			return Path.Combine(_directory, typeof(TKey).Name + ".events.json");
		}

		private string SnapshotFile()
		{
			return Path.Combine(_directory, typeof(TKey).Name + ".snapshots.json");
		}

		private void AppendTo(string filepath, Action<StreamWriter> action)
		{
			using(var fs = _fileSystem.AppendTo(filepath))
			using (var sw = new StreamWriter(fs))
			{
				action(sw);
			}
		}

		private IEnumerable<TDto> ReadFrom<TDto>(string filepath)
		{
			if (_fileSystem.FileExists(filepath) == false)
			{
				return Enumerable.Empty<TDto>();
			}

			return ReadFromImpl<TDto>(filepath);
		}

		private IEnumerable<TDto> ReadFromImpl<TDto>(string filepath)
		{
			using (var fs = _fileSystem.ReadFile(filepath))
			using (var sr = new StreamReader(fs))
			{
				string line;
				while ((line = sr.ReadLine()) != null)
				{
					yield return JsonConvert.DeserializeObject<TDto>(line, _jsonSettings);
				}
			}
		}

		public void SaveEvents(TKey aggegateID, IEnumerable<DomainEvent> changes)
		{
			AppendTo(EventFile(), file =>
			{
				changes.ForEach(change =>
				{
					var dto = new EventDto<TKey> {ID = aggegateID, Event = change};
					var json = JsonConvert.SerializeObject(dto, _jsonSettings);

					file.WriteLine(json);
				});
			});
		}

		public void SaveSnapshot(TKey aggregateID, ISequenced snapshot)
		{
			AppendTo(SnapshotFile(), file =>
			{
				var dto = new SnapshotDto<TKey> {ID = aggregateID, Snapshot = snapshot};
				var json = JsonConvert.SerializeObject(dto, _jsonSettings);

				file.WriteLine(json);
			});
		}

		public int? GetLatestSequenceFor(TKey aggegateID)
		{
			return LoadEvents(aggegateID)
				.Select(e => (int?) e.Sequence)
				.Max();
		}

		public int? GetLatestSnapshotSequenceFor(TKey aggregateID)
		{
			var snapshot = LoadLatestSnapshotFor(aggregateID);

			return snapshot != null
				? snapshot.Sequence
				: (int?)null;
		}

		public IEnumerable<DomainEvent> LoadEvents(TKey aggegateID)
		{
			return ReadFrom<EventDto<TKey>>(EventFile())
				.Where(dto => Equals(dto.ID, aggegateID))
				.Select(dto => dto.Event);
		}

		public IEnumerable<DomainEvent> LoadEventsSince(TKey aggegateID, int sequenceID)
		{
			return LoadEvents(aggegateID)
				.Where(e => e.Sequence > sequenceID);
		}

		public ISequenced LoadLatestSnapshotFor(TKey aggegateID)
		{
			return ReadFrom<SnapshotDto<TKey>>(SnapshotFile())
				.Where(dto => Equals(dto.ID, aggegateID))
				.Select(dto => dto.Snapshot)
				.Cast<ISequenced>()
				.LastOrDefault();
		}
	}
}
