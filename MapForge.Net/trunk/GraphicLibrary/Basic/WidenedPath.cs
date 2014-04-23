using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using GraphicLibrary.Core;

namespace GraphicLibrary.Basic
{
	public class WidenedPath : Shape
	{
		PathGeometryHelper pathHelper = new PathGeometryHelper();
		PathGeometry pathGeoSrc;        // Data flattened
		PathGeometry pathGeoDst;        // Widened path

		// Reusable collections
		List<Point> points = new List<Point>();
		List<Point> ptsOut1 = new List<Point>();
		List<Point> ptsOut2 = new List<Point>();

		// Dependency properties
		public static readonly DependencyProperty DataProperty =
			Path.DataProperty.AddOwner(typeof(WidenedPath),
				new FrameworkPropertyMetadata(null,
						SourcePropertyChanged));

		public static readonly DependencyProperty ToleranceProperty =
			DependencyProperty.Register("Tolerance", typeof(double),
				typeof(WidenedPath),
				new FrameworkPropertyMetadata(0.25,
						SourcePropertyChanged),
				ValidateTolerance);

		static bool ValidateTolerance ( object value )
		{
			return (double)value > 0;
		}

		public static readonly DependencyProperty WideningPenProperty =
			DependencyProperty.Register("WideningPen", typeof(Pen),
				typeof(WidenedPath),
				new FrameworkPropertyMetadata(new Pen(null, 10),
						DestinationPropertyChanged));

		// Dpendency property callbacks
		static void SourcePropertyChanged ( DependencyObject obj,
						DependencyPropertyChangedEventArgs args )
		{
			(obj as WidenedPath).SourcePropertyChanged(args);
		}

		static void DestinationPropertyChanged ( DependencyObject obj,
						DependencyPropertyChangedEventArgs args )
		{
			(obj as WidenedPath).DestinationPropertyChanged(args);
		}

		void SourcePropertyChanged ( DependencyPropertyChangedEventArgs args )
		{
			// Return current source path to cache
			pathHelper.CacheAll(pathGeoSrc);

			// Obtain new path from flattening the geometry
			pathGeoSrc = pathHelper.FlattenGeometry(Data, Tolerance);

			DestinationPropertyChanged(args);
		}

		void DestinationPropertyChanged ( DependencyPropertyChangedEventArgs args )
		{
			// Return current destination path to cache
			pathHelper.CacheAll(pathGeoDst);

			// Obtain new path from generating the geometry
			pathGeoDst = GenerateGeometry(pathGeoSrc, Tolerance);

			InvalidateMeasure();
		}

		// CLR properties
		public Geometry Data
		{
			set { SetValue(DataProperty, value); }
			get { return (Geometry)GetValue(DataProperty); }
		}

		public double Tolerance
		{
			set { SetValue(ToleranceProperty, value); }
			get { return (double)GetValue(ToleranceProperty); }
		}

		public Pen WideningPen
		{
			set { SetValue(WideningPenProperty, value); }
			get { return (Pen)GetValue(WideningPenProperty); }
		}

		// Required override
		protected override Geometry DefiningGeometry
		{
			get
			{
				return pathGeoDst;
			}
		}

		PathGeometry GenerateGeometry ( PathGeometry pathGeoSrc, double tolerance )
		{
			pathGeoDst = pathHelper.GetPathGeometry();

			if (pathGeoSrc == null)
				return pathGeoDst;

			foreach (PathFigure figSrc in pathGeoSrc.Figures) {
				pathHelper.DumpFigureToList(points, figSrc);

				if (points.Count < 2)
					continue;

				PathFigure figDst1, figDst2 = null;
				PolyLineSegment segDst1, segDst2 = null;

				// Get one or two destination PathFigure and PolyLineSegment objects
				figDst1 = pathHelper.GetPathFigure();
				figDst1.IsClosed = true;
				segDst1 = pathHelper.GetPolyLineSegment();

				if (figSrc.IsClosed) {
					figDst2 = pathHelper.GetPathFigure();
					figDst2.IsClosed = true;
					segDst2 = pathHelper.GetPolyLineSegment();
				}

				for (int index = 0; index < points.Count; index++) {
					GetParallelpoints(ptsOut1, points, index, figSrc.IsClosed,
									  WideningPen.Thickness / 2, WideningPen.LineJoin, tolerance);

					GetParallelpoints(ptsOut2, points, index, figSrc.IsClosed,
									  -WideningPen.Thickness / 2, WideningPen.LineJoin, tolerance);

					if (!figSrc.IsClosed) {
						if (index == 0)
							AddCap(segDst1, ptsOut1[0], ptsOut2[0], WideningPen.StartLineCap, tolerance);

						foreach (Point pt in ptsOut1)
							segDst1.Points.Add(pt);

						if (index < points.Count - 1) {
							foreach (Point pt in ptsOut2)
								segDst1.Points.Insert(0, pt);
						} else {
							AddCap(segDst1, ptsOut2[0], ptsOut1[0], WideningPen.EndLineCap, tolerance);
							figDst1.StartPoint = ptsOut2[0];
						}
					} else {
						int start = 0;

						if (index == 0) {
							figDst1.StartPoint = ptsOut1[0];
							figDst2.StartPoint = ptsOut2[0];
							start = 1;
						}

						for (int i = start; i < ptsOut1.Count; i++)
							segDst1.Points.Add(ptsOut1[i]);

						for (int i = start; i < ptsOut2.Count; i++)
							segDst2.Points.Add(ptsOut2[i]);
					}
				}

				figDst1.Segments.Add(segDst1);
				pathGeoDst.Figures.Add(figDst1);

				if (figSrc.IsClosed) {
					figDst2.Segments.Add(segDst2);
					pathGeoDst.Figures.Add(figDst2);
				}
			}
			return pathGeoDst;
		}

		void GetParallelpoints ( List<Point> ptsOut, List<Point> list, int index,
							   bool isClosed, double offset, PenLineJoin join, double tolerance )
		{
			Point ptBefore = new Point();
			Point ptAfter = new Point();
			Point pt, ptNew;

			ptsOut.Clear();

			if (index != 0)
				ptBefore = list[index - 1];

			else if (isClosed)
				ptBefore = list[list.Count - 1];

			pt = list[index];

			if (index < list.Count - 1)
				ptAfter = list[index + 1];

			else if (isClosed)
				ptAfter = list[0];

			System.Windows.Vector v1 = pt - ptBefore;
			v1.Normalize();
			System.Windows.Vector v1Rotated = new System.Windows.Vector(-v1.Y, v1.X);     // 90 degrees

			System.Windows.Vector v2 = ptAfter - pt;
			v2.Normalize();
			System.Windows.Vector v2Rotated = new System.Windows.Vector(-v2.Y, v2.X);      // 90 degrees

			if (index == 0 && !isClosed) {
				ptNew = pt + offset * v2Rotated;
				ptsOut.Add(ptNew);
			} else if (index == list.Count - 1 && !isClosed) {
				ptNew = pt + offset * v1Rotated;
				ptsOut.Add(ptNew);
			} else // if (i < list.Count - 1 || figSrc.IsClosed)
            {
				Point ptBeforeOffset = ptBefore + offset * v1Rotated;
				Point ptAfterOffset = ptAfter + offset * v2Rotated;
				
				Point ptNew1 = pt + offset * v1Rotated;
				Point ptNew2 = pt + offset * v2Rotated;

				ParametricLine2D line1 = new ParametricLine2D(ptNew1, ptBeforeOffset);
				ParametricLine2D line2 = new ParametricLine2D(ptNew2, ptAfterOffset);

				ptNew = line1.SegmentIntersection(line2);

				if (!Double.IsNaN(ptNew.X) && !Double.IsNaN(ptNew.Y)) {
					ptsOut.Add(ptNew);
				} else {
					switch (join) {
						case PenLineJoin.Miter:
							ptNew = line1.Intersection(line2);
							ptsOut.Add(ptNew);
							break;

						case PenLineJoin.Bevel:
							ptsOut.Add(ptNew1);
							ptsOut.Add(ptNew2);
							break;

						case PenLineJoin.Round:
							double angle1 = Math.Atan2(ptNew1.Y - pt.Y, ptNew1.X - pt.X);
							double angle2 = Math.Atan2(ptNew2.Y - pt.Y, ptNew2.X - pt.X);

							if (offset < 0 && angle2 < angle1)
								angle2 += 2 * Math.PI;

							if (offset > 0 && angle2 > angle1)
								angle1 += 2 * Math.PI;

							int max = Math.Max(1, (int)(Math.Abs(4 * offset * (angle2 - angle1) / Math.PI) / tolerance));

							for (int i = 0; i <= max; i++) {
								double angle = (i * angle2 + (max - i) * angle1) / max;
								double x = pt.X + Math.Abs(offset) * Math.Cos(angle);
								double y = pt.Y + Math.Abs(offset) * Math.Sin(angle);
								ptsOut.Add(new Point(x, y));
							}
							break;
					}
				}
			}
		}

		void AddCap ( PolyLineSegment seg, Point pt1, Point pt2, PenLineCap cap, double tolerance )
		{
			if (cap == PenLineCap.Flat)
				return;

			Point midpoint = new Point((pt1.X + pt2.X) / 2,
									   (pt1.Y + pt2.Y) / 2);

			System.Windows.Vector v = pt1 - pt2;
			double l = v.Length;
			v.Normalize();
			v = new System.Windows.Vector(-v.Y, v.X);

			if (cap == PenLineCap.Triangle) {
				seg.Points.Add(midpoint + l / 2 * v);
			} else if (cap == PenLineCap.Square) {
				seg.Points.Add(pt2 + l / 2 * v);
				seg.Points.Add(pt1 + l / 2 * v);
			} else if (cap == PenLineCap.Round) {
				double radStart = Math.Atan2(pt1.Y - pt2.Y, pt2.X - pt1.X);
				int max = (int)((2 * l) / tolerance);

				for (int inc = 1; inc < max; inc += 1) {
					double radians = radStart + inc * Math.PI / max;

					double x = midpoint.X + (l / 2) * Math.Cos(radians);
					double y = midpoint.Y - (l / 2) * Math.Sin(radians);

					seg.Points.Add(new Point(x, y));
				}
			}
		}
	}
}
