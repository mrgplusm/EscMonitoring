using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Monitoring.ViewModel;
using Point = System.Windows.Point;

namespace Monitoring.UserControls
{
    /// <summary>
    /// Interaction logic for ExampleUc.xaml
    /// </summary>
    public partial class UcSchematic : UserControl
    {
        public List<MainUnitViewModel> Nodes { get; set; }

        private SchematicOverView data;

        public UcSchematic()
        {
            InitializeComponent();
            Loaded += UcSchematic_Loaded;
        }

        void UcSchematic_Loaded(object sender, RoutedEventArgs e)
        {
            data = (SchematicOverView)DataContext;
        }

        private void ThumbDrag(object sender, DragDeltaEventArgs e)
        {
            var thumb = sender as Thumb;
            if (thumb == null)
                return;

            var node = thumb.DataContext as DiagramObject;
            if (node == null)
                return;

            var scale = data.PictScalingValue;

            var q = new Point(node.Location.Value.X + e.HorizontalChange * scale, node.Location.Value.Y + e.VerticalChange * scale);

            node.Location.Value = q;
        }

        /// <summary>
        /// Selects the listbox item together with its container when mouse clicks.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UIElement_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!(e.OriginalSource is UIElement)) return;

            var dep = (DependencyObject)e.OriginalSource;

            while ((dep != null) && !(dep is ListBoxItem)) dep = VisualTreeHelper.GetParent(dep);

            var item = dep as ListBoxItem;
            if (dep == null) return;

            if (item == null) return;
            ListBox.SelectedItem = item.DataContext;
        }
    }
}