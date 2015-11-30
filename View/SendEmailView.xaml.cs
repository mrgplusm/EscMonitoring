using System;
using System.Windows;
using System.Windows.Controls;

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
            
            
        }

        private void UIElement_OnGotFocus(object sender, RoutedEventArgs e)
        {
            var q = DataContext as SendEmailViewModel;
            var p = sender as PasswordBox;
            if (q == null || p == null) return;

            q.SenderPassword = string.Empty;
        }
    }


    internal class PasswordBoxMonitor : DependencyObject
    {
        public static bool GetIsMonitoring(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsMonitoringProperty);
        }

        public static void SetIsMonitoring(DependencyObject obj, bool value)
        {
            obj.SetValue(IsMonitoringProperty, value);
        }

        public static readonly DependencyProperty IsMonitoringProperty =
            DependencyProperty.RegisterAttached("IsMonitoring", typeof(bool), typeof(PasswordBoxMonitor), new UIPropertyMetadata(false, OnIsMonitoringChanged));

        public static int GetPasswordLength(DependencyObject obj)
        {
            return (int)obj.GetValue(PasswordLengthProperty);
        }

        public static void SetPasswordLength(DependencyObject obj, int value)
        {
            obj.SetValue(PasswordLengthProperty, value);
        }

        public static readonly DependencyProperty PasswordLengthProperty =
            DependencyProperty.RegisterAttached("PasswordLength", typeof(int), typeof(PasswordBoxMonitor), new UIPropertyMetadata(0));

        private static void OnIsMonitoringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pb = d as PasswordBox;
            if (pb == null)
            {
                return;
            }
            if ((bool)e.NewValue)
            {
                pb.PasswordChanged += PasswordChanged;
            }
            else
            {
                pb.PasswordChanged -= PasswordChanged;
            }
        }

        static void PasswordChanged(object sender, RoutedEventArgs e)
        {
            var pb = sender as PasswordBox;
            if (pb == null)
            {
                return;
            }
            SetPasswordLength(pb, pb.Password.Length);
        }
    }
}