using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using MapCore.Projections;

namespace MapCore
{
	public interface IVectorMapProvider
	{
		MapCore.Projections.RepresentationConverter Projection { get; }
		IMapReadResult ReadMapData ( MapCore.Model.Tile tile );
	}

	public interface IMapReadResult
	{
		bool IsWater { get; }
		System.Collections.Generic.List<MapCore.Model.Node> Nodes { get; }
		System.Collections.Generic.List<MapCore.Model.Way> Ways { get; }
	}

	public interface IBitmapMapProvider
	{
		MapCore.Projections.RepresentationConverter Projection { get; }
		ImageBrush GetTile ( MapCore.Model.Tile tile );
	}
}
