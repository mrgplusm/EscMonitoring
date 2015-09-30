using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Monitoring.UserControls
{
    /// <summary>
    /// Interaction logic for ExternalInput.xaml
    /// </summary>
    public partial class ExternalInput : UserControl
    {
        public ExternalInput()
        {
            InitializeComponent();
            Border.DataContext = this;
        }

        public static readonly DependencyProperty ErrorStoryboardProperty = DependencyProperty.Register(
            "ErrorStoryboard", typeof(Storyboard), typeof(ExternalInput), new PropertyMetadata(default(Storyboard),
                (o, args) =>
                {
                    var z = (ExternalInput)o;
                    var y = (DependencyObject)z.FindName("Border");
                    if (y != null)
                        Storyboard.SetTarget((Storyboard) args.NewValue, y);
                }));

        public Storyboard ErrorStoryboard
        {
            get { return (Storyboard)GetValue(ErrorStoryboardProperty); }
            set { SetValue(ErrorStoryboardProperty, value); }
        }

        public static readonly DependencyProperty IsAvailableInSystemProperty = DependencyProperty.Register(
            "IsAvailableInSystem", typeof(bool), typeof(ExternalInput), new
                PropertyMetadata(default(bool)));

        public bool IsAvailableInSystem
        {
            get { return (bool)GetValue(IsAvailableInSystemProperty); }
            set { SetValue(IsAvailableInSystemProperty, value); }
        }

        public static readonly DependencyProperty DisplayNameProperty = DependencyProperty.Register(
            "DisplayName", typeof(String), typeof(ExternalInput), new PropertyMetadata(default(String)));

        public String DisplayName
        {
            get { return (String)GetValue(DisplayNameProperty); }
            set { SetValue(DisplayNameProperty, value); }
        }

    }


}
