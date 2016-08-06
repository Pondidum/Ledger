using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Ledger.Acceptance.TestObjects;
using Ledger.Infrastructure;
using Ledger.Projection;
using Ledger.Stores;
using Shouldly;
using Xunit;

namespace Ledger.Tests.Examples.InProcessProjection
{
	public class ExampleTests
	{
		private const string StreamName = "TestStream";

		private readonly BridgingProjectionist _projectionist;
		private readonly AggregateStore<Guid> _store;

		public ExampleTests()
		{
			_projectionist = new BridgingProjectionist();

			var backing = new InMemoryEventStore();
			var wrapped = new ProjectionStore(backing, config =>
			{
				config.ProjectTo(_projectionist);
			});

			_store = new AggregateStore<Guid>(wrapped);

			var id = Guid.NewGuid();

			using (var writer = backing.CreateWriter<Guid>(_store.CreateContext(StreamName)))
				writer.SaveEvents(new[]
				{
					new TestEvent { AggregateID = id, Sequence = new Sequence(0) },
					new TestEvent { AggregateID = id, Sequence = new Sequence(1) },
					new TestEvent { AggregateID = id, Sequence = new Sequence(2) },
					new TestEvent { AggregateID = id, Sequence = new Sequence(3) },
					new TestEvent { AggregateID = id, Sequence = new Sequence(4) },
				});
		}

		[Fact]
		public void When_reading_from_the_beginning()
		{
			var projection = new TestProjection();
			var rmb = new ReadModelBuilderService<Guid>(_store, _projectionist, projection);

			rmb.Start(StreamName, StreamSequence.Start);

			projection.Seen().ShouldBe(new[]
			{
				new StreamSequence(0),
				new StreamSequence(1),
				new StreamSequence(2),
				new StreamSequence(3),
				new StreamSequence(4)
			});
		}

		[Fact]
		public void When_reading_from_a_mid_point()
		{
			var projection = new TestProjection();
			var rmb = new ReadModelBuilderService<Guid>(_store, _projectionist, projection);

			rmb.Start(StreamName, new StreamSequence(2));

			projection.Seen().ShouldBe(new[]
			{
				new StreamSequence(3),
				new StreamSequence(4)
			});
		}

		private class TestProjection : Ledger.Projection.Projection
		{
			private readonly AutoResetEvent _reset = new AutoResetEvent(false);
			private readonly List<TestEvent> _events = new List<TestEvent>();

			private void Handle(TestEvent e)
			{
				_events.Add(e);

				if (e.Sequence == new Sequence(4))
					_reset.Set();
			}

			public IEnumerable<StreamSequence> Seen()
			{
				_reset.WaitOne();
				return _events.Select(e => e.StreamSequence);
			}
		}
	}
}
