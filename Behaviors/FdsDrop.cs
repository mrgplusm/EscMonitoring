using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using Common;
using Monitoring.Helpers;
using Monitoring.ViewModel;
using Xceed.Wpf.AvalonDock.Controls;

namespace Monitoring.Behaviors
{
    class FdsDrop : Behavior<ListBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.AllowDrop = true;
            AssociatedObject.DragEnter += AssociatedObject_DragEnter;
            AssociatedObject.DragOver += AssociatedObject_DragOver;
            AssociatedObject.DragLeave += AssociatedObject_DragLeave;
            AssociatedObject.Drop += AssociatedObject_Drop;
        }

        void AssociatedObject_Drop(object sender, DragEventArgs e)
        {
            e.Handled = true;
            if (!e.Data.GetDataPresent(typeof(FDsViewModel))) return;

            var dropContainer = sender as ListBox;
            if (dropContainer == null) return;

            var position = MouseUtilities.GetMousePosition(dropContainer);
            //e.GetPosition(dropContainer);

            var target = AssociatedObject.DataContext as SchematicOverView;
            if (target == null) return;
            var data = e.Data.GetData(typeof(FDsViewModel)) as FDsViewModel;
            if (data == null) return;


            var g = AssociatedObject.GetChildOfType<ScrollViewer>();
            if (g == null) return;
            
            //while ((dep != null) && !(dep is ScrollViewer)) dep = VisualTreeHelper.GetChild(dep);                      
            target.AddToSchematic(data, new Point(position.X + g.HorizontalOffset, position.Y + g.VerticalOffset));
        }

        static void AssociatedObject_DragLeave(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        void AssociatedObject_DragOver(object sender, DragEventArgs e)
        {
            SetDragDropEffects(e);

            var dropContainer = sender as ListBox;
            if (dropContainer == null) return;

            var position = MouseUtilities.GetMousePosition(dropContainer);
            var q = AssociatedObject.FindVisualAncestor<ScrollViewer>();

            var data = e.Data.GetData(typeof(FDsViewModel)) as FDsViewModel;
            if (data == null) return;

            //data.Location.Value = position;

            e.Handled = true;
        }

        static void AssociatedObject_DragEnter(object sender, DragEventArgs e)
        {
            //initialize adorner manager with the adorner layer of the itemsControl
            e.Handled = true;
        }

        /// <summary>
        /// Provides feedback on if the data can be dropped
        /// </summary>
        /// <param name="e"></param>
        private static void SetDragDropEffects(DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(typeof(FDsViewModel)) ? DragDropEffects.Move : DragDropEffects.None;
        }
    }
}