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

        public static readonly DependencyProperty ControlWidthProperty = DependencyProperty.Register(
            "ControlWidth", typeof (double), typeof (ExternalInput), new PropertyMetadata(70D));

        public double ControlWidth
        {
            get { return (double) GetValue(ControlWidthProperty); }
            set { SetValue(ControlWidthProperty, value); }
        }

        public static readonly DependencyProperty ControlHeightProperty = DependencyProperty.Register(
            "ControlHeight", typeof (double), typeof (ExternalInput), new PropertyMetadata(40D));

        public double ControlHeight
        {
            get { return (double) GetValue(ControlHeightProperty); }
            set { SetValue(ControlHeightProperty, value); }
        }

        public static readonly DependencyProperty InactiveTextProperty = DependencyProperty.Register(
            "InactiveText", typeof (string), typeof (ExternalInput), new PropertyMetadata("Not installed"));

        public string InactiveText
        {
            get { return (string) GetValue(InactiveTextProperty); }
            set { SetValue(InactiveTextProperty, value); }
        }
        
    }
}
