using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interactivity;
using Monitoring.Adorners;
using Monitoring.ViewModel;

namespace Monitoring.Behaviors
{
    class FdsDrag : Behavior<Border>
    {
        private bool _isMouseClicked;

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseLeftButtonDown += AssociatedObject_MouseLeftButtonDown;
            AssociatedObject.MouseLeftButtonUp += AssociatedObject_MouseLeftButtonUp;
            AssociatedObject.MouseLeave += AssociatedObject_MouseLeave;
        }

        void AssociatedObject_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isMouseClicked = true;
        }

        void AssociatedObject_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isMouseClicked = false;
        }

        void AssociatedObject_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_isMouseClicked)
            {
                var context = AssociatedObject.DataContext as SchematicOverView;
                if (context == null) return;

                AdornerLayer.GetAdornerLayer(AssociatedObject).Add(new DragAdorner(AssociatedObject));                

                DragDrop.DoDragDrop(AssociatedObject, context.NewFdsModel(), DragDropEffects.Move);
            }
            _isMouseClicked = false;
        }

    }
}