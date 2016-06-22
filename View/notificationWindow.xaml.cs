using System;
using System.Diagnostics;
using System.Media;
using System.Windows;
using Common;
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

            MouseDown += (sender, args) => Close();

            if (!LibraryData.FuturamaSys.SoundEnabled) return;
            try
            {
                var s = new SoundPlayer("error_sound.wav");
                s.PlayLooping();
                Closed += (sender, args) => s.Stop();
                //s.Play();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
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
