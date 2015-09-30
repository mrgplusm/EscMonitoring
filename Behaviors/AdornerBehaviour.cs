using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interactivity;
using Common.Helpers;
using Monitoring.Adorners;
using Monitoring.UserControls;

namespace Monitoring.Behaviors
{
    class ClickBehavior : Behavior<UIElement>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewMouseDown += FdsPanelMouseDown;
            AssociatedObject.PreviewMouseUp += FdsPanelMouseUp;
            AssociatedObject.MouseLeave += FdsMouseLeave;
            AssociatedObject.MouseEnter += MouseEnter;
        }

        private static void MouseEnter(object sender, MouseEventArgs mouseEventArgs)
        {
            AddAdorner(sender);
        }

        private static void AddAdorner(object sender)
        {
            var border = sender as UIElement;
            if (border == null) return;
            AdornerLayer.GetAdornerLayer(border).Add(new ClickAdorner(border));
        }

        private static void FdsPanelMouseDown(object sender, MouseButtonEventArgs e)
        {
            AddAdorner(sender);
        }

        private static void FdsPanelMouseUp(object sender, MouseButtonEventArgs e)
        {
           UIHelpers.RemoveAdorners(sender);
        }


        private static void FdsMouseLeave(object sender, MouseEventArgs e)
        {
           UIHelpers. RemoveAdorners(sender);
        }
    }
}