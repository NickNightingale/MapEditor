using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace MapEditor.AttachedProperties
{
    public class DraggedAdorner : Adorner
	{
		private ContentPresenter _contentPresenter;
		private double _left;
		private double _top;
		private AdornerLayer _adornerLayer;

		public DraggedAdorner(object dragDropData, DataTemplate dragDropTemplate, UIElement adornedElement, AdornerLayer adornerLayer)
			: base(adornedElement)
		{
            _contentPresenter = new ContentPresenter
            {
                Content = dragDropData,
                ContentTemplate = dragDropTemplate,
                Opacity = 0.7,
                IsHitTestVisible = false
            };
            _adornerLayer = adornerLayer;
            _adornerLayer.Add(this);
		}

		public void SetPosition(Point point)
		{
            _left = point.X;
            _top = point.Y;
			if (_adornerLayer != null)
			{
                _adornerLayer.Update(AdornedElement);
			}
		}

		protected override Size MeasureOverride(Size constraint)
		{
            _contentPresenter.Measure(constraint);
			return _contentPresenter.DesiredSize;
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
            _contentPresenter.Arrange(new Rect(finalSize));
			return finalSize;
		}

		protected override Visual GetVisualChild(int index)
		{
			return _contentPresenter;
		}

		protected override int VisualChildrenCount
		{
			get { return 1; }
		}

		public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
		{
			var result = new GeneralTransformGroup();
			result.Children.Add(new TranslateTransform(_left, _top));
			result.Children.Add(base.GetDesiredTransform(transform));
			return result;
		}

		public void Detach()
		{
            _adornerLayer.Remove(this);
		}

	}
}
