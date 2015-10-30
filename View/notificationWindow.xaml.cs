using System;
using System.Media;
using System.Threading.Tasks;
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

            var open = true;

            Task.Factory.StartNew(() =>
            {                
                while (open)
                {
                    SystemSounds.Beep.Play();
                    Task.Delay(1000);
                }
            });

            Closed += (sender, args) => open = false;
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
