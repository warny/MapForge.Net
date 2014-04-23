using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapCore.Projections
{
	public static class Projections
	{
		private static Lazy<MercatorProjection> mercator = new Lazy<MercatorProjection>(() => new MercatorProjection());
		public static MercatorProjection Mercator { get { return mercator.Value; } }

		public static IProjectionTransformation GetProjection ( string name )
		{
			switch (name.ToLower()) {
				case "mercator": 
					return Mercator;
				default:
					throw new ArgumentOutOfRangeException("name", string.Format("La projection \"{0}\" n'est pas supportée", name));
			}
		}
	}
}
