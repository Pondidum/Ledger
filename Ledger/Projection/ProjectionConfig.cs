using System.Collections.Generic;

namespace Ledger.Projection
{
	public class ProjectionConfig
	{
		private readonly List<IProjectionist> _projections;

		public ProjectionConfig()
		{
			_projections = new List<IProjectionist>();
		}

		public void ProjectTo(IProjectionist projectionist)
		{
			_projections.Add(projectionist);
		}

		public void Project<TKey>(DomainEvent<TKey> domainEvent)
		{
			_projections.ForEach(p => p.Project(domainEvent));
		}
	}
}
