using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ledger.Infrastructure;
using Ledger.Projection;

namespace Ledger.Tests.Examples.InProcessProjection
{
	public class ReadModelBuilderService<T> : IDisposable
	{
		private StreamSequence _lastSeen;
		private IEnumerable<DomainEvent<T>> _preload;

		private readonly AggregateStore<T> _store;
		private readonly IProjectionist _projector;
		private readonly BlockingCollection<DomainEvent<T>> _events;
		private readonly CancellationTokenSource _task;

		public ReadModelBuilderService(AggregateStore<T> store, BridgingProjectionist projectionist, IProjectionist projector)
		{
			_store = store;
			_projector = projector;
			_events = new BlockingCollection<DomainEvent<T>>();
			_task = new CancellationTokenSource();

			projectionist.OnEvent<T>(e => _events.Add(e));
		}

		public void Start(string streamName, StreamSequence lastSeen)
		{
			_lastSeen = lastSeen;
			_preload = _store.ReplayAllSince(streamName, _lastSeen);

			Task.Run(() => Process(), _task.Token);
		}

		private void Process()
		{
			foreach (var e in _preload)
			{
				_projector.Apply(e);
				_lastSeen = e.StreamSequence;
			}

			while (_task.IsCancellationRequested == false)
			{
				var e = _events.Take();
				_projector.Apply(e);
				_lastSeen = e.StreamSequence;
			}
		}

		public void Dispose()
		{
			try
			{
				_task.Cancel();
			}
			catch (OperationCanceledException)
			{
			}

			_task.Dispose();
		}
	}
}
