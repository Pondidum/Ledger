using System.Collections.Generic;

namespace Ledger.Projection
{
	public class ProjectorConfig
	{
		private readonly List<IProjectionist> _projections;

		public ProjectorConfig()
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
