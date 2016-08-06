using System.Collections.Generic;

namespace Ledger.Projections
{
	public class ProjectionConfig
	{
		private readonly List<IProjectionist> _projections;

		public ProjectionConfig(List<IProjectionist> projectionists)
		{
			_projections = projectionists;
		}

		public void ProjectTo(IProjectionist projectionist)
		{
			_projections.Add(projectionist);
		}
	}
}
