using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ledger.Infrastructure;

namespace Ledger.Stores
{
	public class ProjectionStoreDecorator : InterceptingEventStore
	{
		private readonly IProjectionist _projectionist;

		public ProjectionStoreDecorator(IEventStore other, IProjectionist projectionist) : base(other)
		{
			_projectionist = projectionist;
		}

		public override IStoreWriter<TKey> CreateWriter<TKey>(EventStoreContext context)
		{
			var other = base.CreateWriter<TKey>(context);

			return new AsyncWriter<TKey>(other, e => _projectionist.Project(e));
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
