using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace GraphicLibrary.Core
{
    public class PointHelper
    {
        Point _value;
        public Point Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public PointHelper(Point Input)
        {
            _value = Input;
        }

        public PointHelper()
        {
        }

        //public static explicit operator Vector(pointHelper point)
        //{
        //    return (new Vector(point._value.X, point._value.Y));
        //}

        public static Vector operator -(PointHelper point1, PointHelper point2)
        {
            return new Vector(point1._value.X - point2._value.X, point1._value.Y - point2._value.Y);
        }

        //
        // Summary:
        //     Translates the specified Point by the specified System.Windows.Vector
        //     and returns the result.
        //
        // Parameters:
        //   point:
        //     The point to translate.
        //
        //   vector:
        //     The amount by which to translate point.
        //
        // Returns:
        //     The result of translating the specified point by the specified vector.
        
        public static PointHelper operator +(PointHelper point, Vector vector)
        {
            PointHelper result = new PointHelper();

            result._value.X = vector.X + point.Value.X;
            result._value.Y = vector.Y + point.Value.Y;

            return result;
        }
    }
}
