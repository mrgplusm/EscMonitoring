using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interactivity;
using System.Windows.Media;
using Common.Model;
using Monitoring.ViewModel;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

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
            if (_tbData == null) return;

            _tb = AssociatedObject.FindName("Header") as TextBlock;
            if (_tb != null)

                SetHeader();

            q.TargetUpdated += (sender, args) =>
            {
                {
                    var bindingExpression = BindingOperations.GetBindingExpression(_tb, TextBlock.TextProperty);
                    bindingExpression?.UpdateTarget();

                    SetHeader();
                }
            };
        }

        public static readonly DependencyProperty ErrorListProperty = DependencyProperty.Register(
            "ErrorList", typeof (List<ErrorLineViewModel>), typeof (UpdateHeader), new PropertyMetadata(default(List<ErrorLineViewModel>)));

        public List<ErrorLineViewModel> ErrorList
        {
            get { return (List<ErrorLineViewModel>) GetValue(ErrorListProperty); }
            set { SetValue(ErrorListProperty, value); }
        }

        /// <summary>
        /// Get status for for grouping bar
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        public ErrorStatuses GetStatus(GroupingError error)
        {                                   
            var e = ErrorList?
                .Where(g => g.DeviceError.Equals(error.Errorline.DeviceError) && g.EscUnit == error.Errorline.EscUnit)
                .OrderBy(g => g.Date)
                .Last();
            return e?.Status ?? ErrorStatuses.FaultSet;            
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