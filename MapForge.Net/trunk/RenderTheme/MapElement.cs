using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using MapCore.Model;
using MapCore.Projections;
using RenderTheme.RenderInstructions;

namespace RenderTheme
{
	public class MapElement
	{
		public PathGeometry WayGeometry { get; private set; }
		public TagList Tags { get; private set; }
		public Point? Position { get; private set; }
		public Tile Tile { get; private set; }
		public bool? IsClosed { get; private set; }
		public List<RenderInstruction> RenderInstructions { get; private set; }

		public MapElement ( RepresentationConverter projection, Tile baseTile, Vector correctionVector, Node node )
		{
			Tags = node.Tags;
			IsClosed = null;
			this.Tile = baseTile;

			WayGeometry = null;
			var mapPoint = (Point)projection.GeoPointToMappoint(node.Position, baseTile.ZoomFactor);
			Position = mapPoint + correctionVector;
			RenderInstructions = new List<RenderInstruction>();
		}

		public MapElement ( RepresentationConverter projection, Tile baseTile, Vector correctionVector, Way way )
		{
			this.Tile = baseTile;
			Tags = way.Tags;
			IsClosed = true;

			PathFigureCollection waySegments = new PathFigureCollection();
			foreach (var geoPointList in way.GeoPoints) {
				PathFigure waySegment = new PathFigure();
				waySegment.Segments = new PathSegmentCollection();
				var geoPointEnumerator = geoPointList.GetEnumerator();
				geoPointEnumerator.MoveNext();

				var mappoint = projection.GeoPointToMappoint(geoPointEnumerator.Current, baseTile.ZoomFactor);
				waySegment.StartPoint = (Point)mappoint - correctionVector;

				Point? lastPoint = null;
				while (geoPointEnumerator.MoveNext()) {
					mappoint = projection.GeoPointToMappoint(geoPointEnumerator.Current, baseTile.ZoomFactor);
					lastPoint = (Point) mappoint - correctionVector;
					waySegment.Segments.Add(new LineSegment(lastPoint.Value, true));
				}
				waySegments.Add(waySegment);
				if (lastPoint == null || lastPoint.Value != waySegment.StartPoint) IsClosed = false;
			}

			WayGeometry = new PathGeometry();
			WayGeometry.Figures = waySegments;

			Point? mapPoint = way.LabelPosition == null ? (Point?)null : (Point)projection.GeoPointToMappoint(way.LabelPosition, baseTile.ZoomFactor);
			Position = mapPoint == null ? null : (mapPoint + correctionVector);

			RenderInstructions = new List<RenderInstruction>();
		}

		public void DrawOld ( DrawingContext context )
		{
			Brush brush = Brushes.Transparent;
			Pen pen = new Pen();

			if (Tags.Match("admin_level")) {
				pen.Brush = Brushes.Black;
			} else if (Tags.Match("waterway")) {
				pen.Brush = Brushes.Blue;
			} else if (Tags.Match("natural=sea")) {
				pen.Brush = Brushes.Transparent;
				brush = Brushes.Blue;
			} else if (Tags.Match("natural=coastline")) {
				pen.Brush = Brushes.Black;
			} else if (Tags.Match("natural=nosea")) {
				pen.Brush = Brushes.Transparent;
				brush = Brushes.White;
			} else if (Tags.Match("natural=water")) {
				pen.Brush = Brushes.Transparent;
				brush = Brushes.Cyan;
			} else if (Tags.Match("highway")) {
				pen.Brush = Brushes.Maroon;
			} else if (Tags.Match("railway")) {
				pen.Brush = Brushes.Magenta;
			} else if (Tags.Match("landuse=farm")) {
				pen.Brush = Brushes.Transparent;
				brush = Brushes.Yellow;
			} else if (Tags.Match("landuse=meadow")) {
				pen.Brush = Brushes.Transparent;
				brush = Brushes.LightGreen;
			} else if (Tags.Match("landuse=forest")) {
				pen.Brush = Brushes.Transparent;
				brush = Brushes.Green;
			} else if (Tags.Match("landuse=basin")) {
				pen.Brush = Brushes.Transparent;
				brush = Brushes.Blue;
			} else if (Tags.Match("leisure=swimming_pool")) {
				pen.Brush = Brushes.Blue;
				brush = Brushes.Cyan;
			} else if (Tags.Match("leisure=park")) {
				pen.Brush = Brushes.Green;
				brush = Brushes.GreenYellow;
			} else if (Tags.Match("leisure=playground")) {
				pen.Brush = Brushes.Green;
				brush = Brushes.Firebrick;
			} else if (Tags.Match("route")) {
				pen.Brush = Brushes.Red;
			} else if (Tags.Match("building")) {
				brush = Brushes.Gray;
				pen.Brush = Brushes.Black;
			} else if (Tags.Match("contour_ext")) {
				pen.Brush = Brushes.LightGray;
			} else {
				//System.Diagnostics.Debug.WriteLine(Tags);
				//pen.Brush =Brushes.Black;
			}

			context.DrawGeometry(brush, pen, WayGeometry);

			//foreach (var figure in WayGeometry.Figures) {
			//	var f = figure.GetFlattenedPathFigure();
			//}

			pen.Thickness = 1;
		}

		public void Draw ( DrawingContext context )
		{
			RenderInstruction lastInstruction = null;
			foreach (var renderInstruction in RenderInstructions) {
				if (renderInstruction.Equals(lastInstruction)) continue;
				renderInstruction.RenderWay(context, this);
				lastInstruction = renderInstruction;
			}
			foreach (var renderInstruction in RenderInstructions) {
				if (renderInstruction.Equals(lastInstruction)) continue;
				renderInstruction.RenderNode(context, this);
				lastInstruction = renderInstruction;
			}
		}


	}

}
