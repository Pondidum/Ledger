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
			var wrapped = new ProjectionStoreDecorator(backing, _projectionist);

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
			var reset = new AutoResetEvent(false);
			var seen = new List<StreamSequence>();

			var projector = new Projector<Guid>();
			projector.Register<TestEvent>(e =>
			{
				seen.Add(e.StreamSequence);

				if (e.Sequence == new Sequence(4))
					reset.Set();
			});

			var rmb = new ReadModelBuilderService<Guid>(_store, _projectionist, projector);

			rmb.Start(StreamName, StreamSequence.Start);

			//async funsies
			reset.WaitOne();

			seen.ShouldBe(new[]
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
			var reset = new AutoResetEvent(false);
			var seen = new List<StreamSequence>();

			var projector = new Projector<Guid>();
			projector.Register<TestEvent>(e =>
			{
				seen.Add(e.StreamSequence);

				if (e.Sequence == new Sequence(4))
					reset.Set();
			});

			var rmb = new ReadModelBuilderService<Guid>(_store, _projectionist, projector);

			rmb.Start(StreamName, new StreamSequence(2));

			//async funsies
			reset.WaitOne();

			seen.ShouldBe(new[]
			{
				new StreamSequence(3),
				new StreamSequence(4)
			});
		}
	}
}
