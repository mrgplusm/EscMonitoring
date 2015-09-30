using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using Monitoring.ViewModel;

namespace Monitoring.View

{
    /// <summary>
    ///     Interaction logic for SendEmailView.xaml
    /// </summary>
    public partial class SendEmailView 
    {
        public SendEmailView()
        {
            InitializeComponent();
            
        

        }


        private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            var q =  DataContext as SendEmailViewModel;
            var p = sender as PasswordBox;
            if(q == null || p == null) return;

            q.SenderPassword = p.Password;
            //kutje
            int i = 0;
        }
    }
}