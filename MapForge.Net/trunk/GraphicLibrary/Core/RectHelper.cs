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
    public class RectHelper
    {
        Rect _value;
        public Rect Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public RectHelper(Rect Input)
        {
            _value = Input;
        }

        public RectHelper()
        {
        }

        // Summary:
        //     Gets the position of the top-left corner of the rectangle.
        //
        // Returns:
        //     The position of the top-left corner of the rectangle.
        public Point TopLeft
        {
            get { return (new Point(_value.X, _value.Y)); }
        }
      
        //
        // Summary:
        //     Gets the position of the top-right corner of the rectangle.
        //
        // Returns:
        //     The position of the top-right corner of the rectangle.
        public Point TopRight
        {
            get { return (new Point(_value.X + _value.Width, _value.Y)); }
        }

        //
        // Summary:
        //     Gets the position of the bottom-left corner of the rectangle
        //
        // Returns:
        //     The position of the bottom-left corner of the rectangle.
        public Point BottomLeft
        {
            get { return (new Point(_value.X, _value.Y + _value.Height)); }
        }
   
        //
        // Summary:
        //     Gets the position of the bottom-right corner of the rectangle.
        //
        // Returns:
        //     The position of the bottom-right corner of the rectangle.
        public Point BottomRight
        {
            get { return (new Point(_value.X + _value.Width, _value.Y + _value.Height)); }
        }
   

    }
}
