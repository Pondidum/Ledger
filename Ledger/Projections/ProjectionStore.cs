using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ledger.Infrastructure;
using Ledger.Stores;

namespace Ledger.Projections
{
	public class ProjectionStore : IEventStore
	{
		private readonly IEventStore _other;
		private readonly List<IProjectionist> _projectionists;

		public ProjectionStore(IEventStore other, Action<ProjectionConfig> configure)
		{
			_other = other;
			_projectionists = new List<IProjectionist>();

			configure(new ProjectionConfig(_projectionists));
		}

		public IStoreReader<TKey> CreateReader<TKey>(EventStoreContext context)
		{
			return _other.CreateReader<TKey>(context);
		}

		public IStoreWriter<TKey> CreateWriter<TKey>(EventStoreContext context)
		{
			var other = _other.CreateWriter<TKey>(context);

			return new AsyncWriter<TKey>(other, RunProjections);
		}

		private void RunProjections<TKey>(DomainEvent<TKey> domainEvent)
		{
			_projectionists.ForEach(p => p.Apply(domainEvent));
		}

		private class AsyncWriter<T> : InterceptingWriter<T>
		{
			private readonly BlockingCollection<DomainEvent<T>> _events;
			private readonly CancellationTokenSource _task;

			public AsyncWriter(IStoreWriter<T> other, Action<DomainEvent<T>> projection)
				: base(other)
			{
				_events = new BlockingCollection<DomainEvent<T>>();
				_task = new CancellationTokenSource();

				Task.Run(
					() =>
					{
						while (_task.IsCancellationRequested == false)
							projection(_events.Take());
					},
					_task.Token
				);
			}

			public override void SaveEvents(IEnumerable<DomainEvent<T>> changes)
			{
				base.SaveEvents(changes.Apply(e => _events.Add(e)));
			}

			public override void Dispose()
			{
				try
				{
					_task.Cancel();
				}
				catch (OperationCanceledException)
				{
				}

				_task.Dispose();
				base.Dispose();
			}
		}
	}
}
