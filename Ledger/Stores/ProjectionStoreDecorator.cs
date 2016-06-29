using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ledger.Infrastructure;

namespace Ledger.Stores
{
	public class ProjectionStoreDecorator : InterceptingEventStore
	{
		private readonly IProjectionist _projectionist;

		public ProjectionStoreDecorator(IEventStore other, IProjectionist projectionist ) : base(other)
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
			private readonly Action<DomainEvent<T>> _projection;

			public AsyncWriter(IStoreWriter<T> other, Action<DomainEvent<T>> projection)
				: base(other)
			{
				_projection = projection;
			}

			public override void SaveEvents(IEnumerable<DomainEvent<T>> changes)
			{
				var toRaise = new List<DomainEvent<T>>();

				base.SaveEvents(changes.Apply(e => toRaise.Add(e)));

				Task.Run(() =>
				{
					foreach (var domainEvent in toRaise)
						_projection(domainEvent);
				});
			}
		}
	}
}
