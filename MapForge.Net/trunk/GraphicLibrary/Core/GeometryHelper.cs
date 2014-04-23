﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GraphicLibrary.Core
{
    public static class GeometryHelper
    {
        public static List<Point> GetIntersectionpoints(PathGeometry FlattenedPath, double[] SegmentLengths)
        {
            List<Point> intersectionPoints = new List<Point>();

            List<Point> pointsOnFlattenedPath = GetpointsOnFlattenedPath(FlattenedPath);

            if (pointsOnFlattenedPath == null || pointsOnFlattenedPath.Count < 2)
                return intersectionPoints;

            Point currpoint = pointsOnFlattenedPath[0];
            intersectionPoints.Add(currpoint);

            // find point on flattened path that is segment length away from current point

            int flattedPathIndex = 0;

            int segmentIndex = 1;

            while (flattedPathIndex < pointsOnFlattenedPath.Count - 1 &&
                segmentIndex < SegmentLengths.Length + 1)
            {
                Point? intersectionPoint = GetIntersectionOfSegmentAndCircle(
                    pointsOnFlattenedPath[flattedPathIndex],
                    pointsOnFlattenedPath[flattedPathIndex + 1], currpoint, SegmentLengths[segmentIndex - 1]);

                if (intersectionPoint == null)
                    flattedPathIndex++;
                else
                {
                    intersectionPoints.Add((Point)intersectionPoint);
                    currpoint = (Point)intersectionPoint;
                    pointsOnFlattenedPath[flattedPathIndex] = currpoint;
                    segmentIndex++;
                }
            }

            return intersectionPoints;
        }

        static List<Point> GetpointsOnFlattenedPath(PathGeometry FlattenedPath)
        {
            List<Point> flattenedPathpoints = new List<Point>();

            // for flattened geometry there should be just one PathFigure in the Figures
            if (FlattenedPath.Figures.Count != 1)
                return null;

            PathFigure pathFigure = FlattenedPath.Figures[0];

            flattenedPathpoints.Add(pathFigure.StartPoint);

            // SegmentsCollection should contain PolyLineSegment and LineSegment
            foreach (PathSegment pathSegment in pathFigure.Segments)
            {
                if (pathSegment is PolyLineSegment)
                {
                    PolyLineSegment seg = pathSegment as PolyLineSegment;

                    foreach (Point point in seg.Points)
                        flattenedPathpoints.Add(point);
                }
                else if (pathSegment is LineSegment)
                {
                    LineSegment seg = pathSegment as LineSegment;

                    flattenedPathpoints.Add(seg.Point);
                }
                else
                    throw new Exception("GetIntersectionpoint - unexpected path segment type: " + pathSegment.ToString());

            }

            return (flattenedPathpoints);
        }

        static Point? GetIntersectionOfSegmentAndCircle(Point Segmentpoint1, Point Segmentpoint2,
            Point CircleCenter, double CircleRadius)
        {
            // linear equation for segment: y = mx + b
            double slope = (Segmentpoint2.Y - Segmentpoint1.Y) / (Segmentpoint2.X - Segmentpoint1.X);
            double intercept = Segmentpoint1.Y - (slope * Segmentpoint1.X);

            // special case when segment is vertically oriented
            if (double.IsInfinity(slope))
            {
                double root = Math.Pow(CircleRadius, 2.0) - Math.Pow(Segmentpoint1.X - CircleCenter.X, 2.0);

                if (root < 0)
                    return null;

                // soln 1
                double SolnX1 = Segmentpoint1.X;
                double SolnY1 = CircleCenter.Y - Math.Sqrt(root);
                Point Soln1 = new Point(SolnX1, SolnY1);

                // have valid result if point is between two segment points
                if (IsBetween(SolnX1, Segmentpoint1.X, Segmentpoint2.X) &&
                    IsBetween(SolnY1, Segmentpoint1.Y, Segmentpoint2.Y))
                //if (ValidSoln(Soln1, Segmentpoint1, Segmentpoint2, CircleCenter))
                {
                    // found solution
                    return (Soln1);
                }

                // soln 2
                double SolnX2 = Segmentpoint1.X;
                double SolnY2 = CircleCenter.Y + Math.Sqrt(root);
                Point Soln2 = new Point(SolnX2, SolnY2);

                // have valid result if point is between two segment points
                if (IsBetween(SolnX2, Segmentpoint1.X, Segmentpoint2.X) &&
                    IsBetween(SolnY2, Segmentpoint1.Y, Segmentpoint2.Y))
                //if (ValidSoln(Soln2, Segmentpoint1, Segmentpoint2, CircleCenter))
                {
                    // found solution
                    return (Soln2);
                }
            }
            else
            {
                // use soln to quadradratic equation to solve intersection of segment and circle:
                // x = (-b +/ sqrt(b^2-4ac))/(2a)
                double a = 1 + Math.Pow(slope, 2.0);
                double b = (-2 * CircleCenter.X) + (2 * (intercept - CircleCenter.Y) * slope);
                double c = Math.Pow(CircleCenter.X, 2.0) + Math.Pow(intercept - CircleCenter.Y, 2.0) - Math.Pow(CircleRadius, 2.0);

                // check for no solutions, is sqrt negative?
                double root = Math.Pow(b, 2.0) - (4 * a * c);

                if (root < 0)
                    return null;

                // we might have two solns...

                // soln 1
                double SolnX1 = (-b + Math.Sqrt(root)) / (2 * a);
                double SolnY1 = slope * SolnX1 + intercept;
                Point Soln1 = new Point(SolnX1, SolnY1);

                // have valid result if point is between two segment points
                if (IsBetween(SolnX1, Segmentpoint1.X, Segmentpoint2.X) &&
                    IsBetween(SolnY1, Segmentpoint1.Y, Segmentpoint2.Y))
                //if (ValidSoln(Soln1, Segmentpoint1, Segmentpoint2, CircleCenter))
                {
                    // found solution
                    return (Soln1);
                }

                // soln 2
                double SolnX2 = (-b - Math.Sqrt(root)) / (2 * a);
                double SolnY2 = slope * SolnX2 + intercept;
                Point Soln2 = new Point(SolnX2, SolnY2);

                // have valid result if point is between two segment points
                if (IsBetween(SolnX2, Segmentpoint1.X, Segmentpoint2.X) &&
                    IsBetween(SolnY2, Segmentpoint1.Y, Segmentpoint2.Y))
                //if (ValidSoln(Soln2, Segmentpoint1, Segmentpoint2, CircleCenter))
                {
                    // found solution
                    return (Soln2);
                }
            }

            // shouldn't get here...but in case
            return null;
        }

        static bool IsBetween(double X, double X1, double X2)
        {
            if (X1 >= X2 && X <= X1 && X >= X2)
                return true;

            if (X1 <= X2 && X >= X1 && X <= X2)
                return true;

            return false;
        }
    }
}
