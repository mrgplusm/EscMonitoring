using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Monitoring.Adorners
{
    class DragAdorner : Adorner
    {

        private readonly System.Windows.Shapes.Rectangle _child;

        public DragAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
            var size = adornedElement.RenderSize;
            var rect = new System.Windows.Shapes.Rectangle
            {
                Fill = new VisualBrush(adornedElement), 
                Width = size.Width, 
                Height = size.Height
            };

            _child = rect;
        }

        protected override Visual GetVisualChild(int index)
        {
            return this._child;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            this._child.Arrange(new Rect(finalSize));
            return finalSize;
        }

      

        protected override Size MeasureOverride(Size constraint)
        {
            this._child.Measure(constraint);
            return this._child.DesiredSize;
        }

        /// <summary>
        /// Override.  Always returns 1.
        /// </summary>
        protected override int VisualChildrenCount
        {
            get { return 1; }
        }
    }
}