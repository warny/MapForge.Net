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
using RenderTheme.Graphics;
using System.Xml;
using System.IO;

namespace MapsUtilities.Overlay
{
	public static class OverlayUtils
	{
		public static void SetShapeFormat ( Paint paintStroke, Paint paintFill, System.Windows.Shapes.Shape shape )
		{
			if (paintStroke != null) {
				SolidColorBrush stroke = new SolidColorBrush();
				stroke.Color = paintStroke.Color;
				shape.Stroke = stroke;
			}
			if (paintFill != null) {
				SolidColorBrush fill = new SolidColorBrush();
				fill.Color = paintFill.Color;
				shape.Fill = fill;
			}
		}

		public static Rect GetPosition ( this Shape shape )
		{
			TranslateTransform t = shape.RenderTransform as TranslateTransform;
			Rect rect = new Rect(
				Canvas.GetLeft(shape) + (t == null ? 0 : t.X),
				Canvas.GetTop(shape) + (t == null ? 0 : t.Y),
				shape.Width,
				shape.Height
				);
			return rect;
		}

		public static T Clone<T> ( this T shape ) where T : Shape
		{
			using (MemoryStream s = new MemoryStream()) {
				System.Runtime.Serialization.DataContractSerializer serializer = new System.Runtime.Serialization.DataContractSerializer(shape.GetType());
				serializer.WriteObject(s, shape);
				s.Seek(0, SeekOrigin.Begin);
				T clone = (T)serializer.ReadObject(s);
				return clone;
			}

		}

	}
}
