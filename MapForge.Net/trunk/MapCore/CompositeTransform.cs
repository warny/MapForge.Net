using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace System.Windows.Media
{
	public class CompositeTransform : GeneralTransform
	{
		ScaleTransform scale;
		SkewTransform skew;
		RotateTransform rotate;
		TranslateTransform translate;

		TransformGroup group;
		public CompositeTransform ()
		{
			scale = new ScaleTransform();
			skew = new SkewTransform();
			rotate = new RotateTransform();
			translate = new TranslateTransform();

			group = new TransformGroup();
			group.Children.Add(scale);
			group.Children.Add(skew);
			group.Children.Add(rotate);
			group.Children.Add(translate);
		}

		public double Rotation
		{
			get { return rotate.Angle; }
			set { rotate.Angle = value; }
		}

		public double ScaleX
		{
			get { return scale.ScaleX; }
			set { scale.ScaleX = value; }
		}

		public double ScaleY
		{
			get { return scale.ScaleY; }
			set { scale.ScaleY = value; }
		}

		public double SkewX
		{
			get { return skew.AngleX; }
			set { skew.AngleX = value; }
		}

		public double SkewY
		{
			get { return skew.AngleY; }
			set { skew.AngleY = value; }
		}

		public double TranslateX
		{
			get { return translate.X; }
			set { translate.X = value; }
		}

		public double TranslateY
		{
			get { return translate.Y; }
			set { translate.Y = value; }
		}

		public double CenterX
		{
			get { return rotate.CenterX; }
			set { 
				rotate.CenterX = value;
				scale.CenterX = value;
				skew.CenterX = value;
			}
		}
		public double CenterY
		{
			get { return rotate.CenterY; }
			set
			{
				rotate.CenterY = value;
				scale.CenterY = value;
				skew.CenterY = value;
			}
		}


		public override GeneralTransform Inverse
		{
			get { return group.Inverse; }
		}

		public override Rect TransformBounds ( Rect rect )
		{
			return group.TransformBounds(rect);
		}

		public override bool TryTransform ( Point inpoint, out Point result )
		{
			return group.TryTransform(inpoint, out result);
		}

		protected override Freezable CreateInstanceCore ()
		{
			return group.GetAsFrozen();
		}
	}
}
