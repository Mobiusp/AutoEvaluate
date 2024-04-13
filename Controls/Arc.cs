using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AutoEvaluate.Controls
{
    public class Arc : Shape
    {
        private Geometry geometry
        {
            get { return (Geometry)GetValue(GeometryProperty); }
            set { SetValue(GeometryProperty, value); }
        }

        public static readonly DependencyProperty GeometryProperty =
            DependencyProperty.Register("Geometry", typeof(Geometry), typeof(Arc), new PropertyMetadata((Geometry)Geometry.Parse(""), (s, e) =>
            {
                if (s is Arc arc && e.NewValue.ToString() != e.OldValue.ToString())
                {
                    arc.InvalidateVisual();
                }
            }));

        protected override Geometry DefiningGeometry { get { return (Geometry)GetValue(GeometryProperty); } }

        public double Diameter
        {
            get { return (double)GetValue(DiameterProperty); }
            set { SetValue(DiameterProperty, value); }
        }

        public static readonly DependencyProperty DiameterProperty =
            DependencyProperty.Register("Diameter", typeof(double), typeof(Arc), new PropertyMetadata(0.0, PropertyChanged));

        public double StartAngle
        {
            get { return (double)GetValue(StartAngleProperty); }
            set { SetValue(StartAngleProperty, value); }
        }

        public static readonly DependencyProperty StartAngleProperty =
            DependencyProperty.Register("StartAngle", typeof(double), typeof(Arc), new PropertyMetadata(0.0, PropertyChanged));

        public double EndAngle
        {
            get { return (double)GetValue(EndAngleProperty); }
            set { SetValue(EndAngleProperty, value); }
        }

        public static readonly DependencyProperty EndAngleProperty =
            DependencyProperty.Register("EndAngle", typeof(double), typeof(Arc), new PropertyMetadata(0.0, PropertyChanged));

        public static void PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (! (d is Arc)) return;
            Arc arc = (Arc)d;
            double radius = arc.Diameter / 2;
            if (radius <= 0.0) return;
            if (arc.StartAngle >= arc.EndAngle ||
                arc.StartAngle < 0 || arc.StartAngle > 360 ||
                arc.EndAngle < 0 || arc.EndAngle > 360) 
            {
                arc.geometry = (Geometry)Geometry.Parse(String.Empty);
                return;
            }

            arc.Width = (radius + arc.StrokeThickness) * 2;
            arc.Height = (radius + arc.StrokeThickness) * 2;

            double startX = radius * (1 + Math.Sin(arc.StartAngle / 180 * Math.PI));
            double startY = radius * (1 - Math.Cos(arc.StartAngle / 180 * Math.PI));
            double endX = radius * (1 + Math.Sin(arc.EndAngle / 180 * Math.PI));
            double endY = radius * (1 - Math.Cos(arc.EndAngle / 180 * Math.PI));
            int isLarge = (arc.EndAngle - arc.StartAngle > 180 ? 1 : 0);
            string geometry = $"M{startX + 0.0001 + arc.StrokeThickness},{startY + arc.StrokeThickness} A{radius},{radius} 0,{isLarge},1 {endX + arc.StrokeThickness},{endY + arc.StrokeThickness}";
            arc.geometry = (Geometry)Geometry.Parse(geometry);
        }
    }
}
