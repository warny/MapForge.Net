using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GraphicLibrary.Core
{
	// represents line as x = x1 + (x2 - x1)t
	//                    y = y1 + (y2 - y1)t
	public struct ParametricLine2D
	{
		double x1, x2, y1, y2;

		public double X1
		{
			set { x1 = value; }
			get { return x1; }
		}

		public double X2
		{
			set { x2 = value; }
			get { return x2; }
		}

		public double Y1
		{
			set { y1 = value; }
			get { return y1; }
		}

		public double Y2
		{
			set { y2 = value; }
			get { return y2; }
		}

		public ParametricLine2D ( Point pt1, Point pt2 )
		{
			x1 = x2 = y1 = y2 = 0;

			X1 = pt1.X;
			Y1 = pt1.Y;
			X2 = pt2.X;
			Y2 = pt2.Y;
		}

		public Point Intersection ( ParametricLine2D line )
		{
			double tThis, tThat;

			IntersectTees(line, out tThis, out tThat);

			return new Point(X1 + tThis * (X2 - X1),
							 Y1 + tThis * (Y2 - Y1));
		}

		public Point SegmentIntersection ( ParametricLine2D line )
		{
			double tThis, tThat;

			IntersectTees(line, out tThis, out tThat);

			if (tThis < 0 || tThis > 1 || tThat < 0 || tThat > 1)
				return new Point(Double.NaN, Double.NaN);

			return new Point(X1 + tThis * (X2 - X1),
							 Y1 + tThis * (Y2 - Y1));
		}

		void IntersectTees ( ParametricLine2D line, out double tThis, out double tThat )
		{
			double den = (line.Y2 - line.Y1) * (X2 - X1) - (line.X2 - line.X1) * (Y2 - Y1);

			tThis = ((line.X2 - line.X1) * (Y1 - line.Y1) - (line.Y2 - line.Y1) * (X1 - line.X1)) / den;
			tThat = ((X2 - X1) * (Y1 - line.Y1) - (Y2 - Y1) * (X1 - line.X1)) / den;
		}

		public override string ToString ()
		{
			return String.Format("X1 = {0} X2 = {1} Y1 = {2} Y2 = {3}", X1, X2, Y1, Y2);
		}
	}
}
