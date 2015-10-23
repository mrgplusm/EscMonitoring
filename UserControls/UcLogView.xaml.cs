using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interactivity;
using System.Windows.Media;
using Common.Model;
using Monitoring.ViewModel;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;

namespace Monitoring.UserControls
{
    internal class UpdateHeader : Behavior<Expander>
    {
        TextBlock _tb;
        private TextBlock _tbData;

        protected override void OnAttached()
        {
            base.OnAttached();

            var grid = AssociatedObject.FindName("Grid") as DataGrid;

            var q = AssociatedObject.FindName("Items") as TextBlock;
            if (q == null) return;

            _tbData = AssociatedObject.FindName("TbData") as TextBlock;
            if (_tbData == null) return;

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

        /// <summary>
        /// Get status for for grouping bar
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        public ErrorStatuses GetStatus(GroupingError error)
        {
            //todo: make something nicer??
            var q = ViewModelLocator.MainView.ErrorList;

            var e = q
                .Where(g => g.DeviceError.Equals(error.Errorline.DeviceError) && g.EscUnit == error.Errorline.EscUnit)
                .OrderBy(g => g.Date)
                .Last();
            return e == null ? ErrorStatuses.FaultSet : e.Status;
        }

        private void SetHeader()
        {
            var status = GetStatus(_tbData.DataContext as GroupingError);

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
        }        
    }
}