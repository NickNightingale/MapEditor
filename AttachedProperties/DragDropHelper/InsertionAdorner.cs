using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace MapEditor.AttachedProperties
{
    public class InsertionAdorner : Adorner
	{
        private RelativePosition _relativePosition;
        public RelativePosition RelativePosition
        {
            get { return _relativePosition; }
            set
            {
                if (_relativePosition != value)
                {
                    _relativePosition = value;
                    InvalidateVisual();
                }
             }
                
        }

        private Orientation _orientation;
		private AdornerLayer _adornerLayer;
		private static Pen _pen;
		private static PathGeometry _triangle;

		// Create the pen and triangle in a static constructor and freeze them to improve performance.
		static InsertionAdorner()
		{
			_pen = new Pen { Brush = Brushes.Gray, Thickness = 2 };
			_pen.Freeze();

			var firstLine = new LineSegment(new Point(0, -5), false);
			firstLine.Freeze();
            var secondLine = new LineSegment(new Point(0, 5), false);
			secondLine.Freeze();

            var figure = new PathFigure { StartPoint = new Point(5, 0) };
			figure.Segments.Add(firstLine);
			figure.Segments.Add(secondLine);
			figure.Freeze();

			_triangle = new PathGeometry();
			_triangle.Figures.Add(figure);
			_triangle.Freeze();
		}

		public InsertionAdorner(
            Orientation orientation, 
            RelativePosition relativePosition, 
            UIElement adornedElement, 
            AdornerLayer adornerLayer)
			: base(adornedElement)
		{
            _orientation = orientation;
            RelativePosition = relativePosition;
            IsHitTestVisible = false;

            _adornerLayer = adornerLayer;
            _adornerLayer.Add(this);
		}

		// This draws one line and two triangles at each end of the line.
		protected override void OnRender(DrawingContext drawingContext)
		{
			Point startPoint;
			Point endPoint;
			CalculateStartAndEndPoint(out startPoint, out endPoint);
			drawingContext.DrawLine(_pen, startPoint, endPoint);
            switch(_orientation)
            {
                case Orientation.Horizontal:
                    DrawTriangle(drawingContext, startPoint, 0);
                    DrawTriangle(drawingContext, endPoint, 180);
                    break;
                case Orientation.Vertical:
                    DrawTriangle(drawingContext, startPoint, 90);
                    DrawTriangle(drawingContext, endPoint, -90);
                    break;
            }
		}

		private void DrawTriangle(DrawingContext drawingContext, Point origin, double angle)
		{
			drawingContext.PushTransform(new TranslateTransform(origin.X, origin.Y));
			drawingContext.PushTransform(new RotateTransform(angle));
			drawingContext.DrawGeometry(_pen.Brush, null, _triangle);
			drawingContext.Pop();
			drawingContext.Pop();
		}

		private void CalculateStartAndEndPoint(out Point startPoint, out Point endPoint)
		{
			startPoint = new Point();
			endPoint = new Point();
			
			var width = AdornedElement.RenderSize.Width;
			var height = AdornedElement.RenderSize.Height;

            switch (_orientation)
            {
                case Orientation.Horizontal:
                    endPoint.X = width;
                    if (RelativePosition == RelativePosition.After)
                    {
                        startPoint.Y = height;
                        endPoint.Y = height;
                    }
                    break;
                case Orientation.Vertical:
                    endPoint.Y = height;
                    if (RelativePosition == RelativePosition.After)
                    {
                        startPoint.X = width;
                        endPoint.X = width;
                    }
                    break;
            }
		}

		public void Detach()
		{
            _adornerLayer.Remove(this);
		}

	}

    public enum RelativePosition
    {
        Before,
        After
    }
}
