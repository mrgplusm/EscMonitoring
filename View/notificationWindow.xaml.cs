using System;
using System.Windows;
using System.Windows.Threading;
using Monitoring.ViewModel;

namespace Monitoring.View
{
    /// <summary>
    /// Interaction logic for notificationWindow.xaml
    /// </summary>
    public partial class NotificationWindow : Window
    {
        public NotificationWindow()
        {
            
            InitializeComponent();

         //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
         //   {
         //       var workingArea = SystemParameters.WorkArea;
         //       Left = workingArea.Height - Height;
         //       Top = workingArea.Width - Width;
         //   }));
            
            MouseDown += (sender, args) =>  Close();
        }

        public static readonly DependencyProperty ErrorLineProperty = DependencyProperty.Register(
            "ErrorLine", typeof(ErrorLineViewModel), typeof(NotificationWindow), new PropertyMetadata(default(ErrorLineViewModel)));

        public ErrorLineViewModel ErrorLine
        {
            get { return (ErrorLineViewModel)GetValue(ErrorLineProperty); }
            set { SetValue(ErrorLineProperty, value); }
        }

    }
}
