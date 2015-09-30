using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interactivity;
using System.Windows.Media;
using Common.Model;
using Monitoring.ViewModel;

namespace Monitoring.UserControls
{
    internal class UpdateHeader : Behavior<Expander>
    {
        TextBlock _tb;
        private TextBlock _tbData;

        protected override void OnAttached()
        {
            base.OnAttached();

            var q = AssociatedObject.FindName("Items") as TextBlock;
            if (q == null) return;

            _tbData = AssociatedObject.FindName("TbData") as TextBlock;
            if(_tbData ==  null) return;

            _tb = AssociatedObject.FindName("Header") as TextBlock;
            if (_tb != null)

                SetHeader();

            q.TargetUpdated += (sender, args) =>
            {
                {
                    var bindingExpression = BindingOperations.GetBindingExpression(_tb, TextBlock.TextProperty);
                    if (bindingExpression != null)
                        bindingExpression.UpdateTarget();

                    SetHeader();
                }
            };
        }

        private void SetHeader()
        {
            var status = MainViewModel.GetStatus(_tbData.DataContext as GroupingError);

            if (status == ErrorStatuses.FaultSet)
            {
                _tb.Foreground = Brushes.Orange;
                AssociatedObject.IsExpanded = true;
            }
            else
            {
                _tb.Foreground = Brushes.Green;
                AssociatedObject.IsExpanded = false;
            }
        }
    }


    /// <summary>
    ///     Interaction logic for MonitorWindow.xaml
    /// </summary>
    public partial class UcLogView
    {
        public UcLogView()
        {
            InitializeComponent();
            //var sd = grid1.Items.SortDescriptions[0];
            //sd.Direction = ListSortDirection.Descending;
            //grid1.Items.SortDescriptions[0] = sd;

            //Grid.DataContextChanged += Grid_DataContextChanged;


        }

        //void Grid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        //{

        //    Grid.Items.SortDescriptions.Add(new SortDescription("Id", ListSortDirection.Descending));

        //}


    }



}