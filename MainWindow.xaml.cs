using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using Monitoring.View;
using Monitoring.ViewModel;
using Xceed.Wpf.AvalonDock.Controls;

using System.Windows.Media;

namespace Monitoring
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private readonly KeyBinding _changePasswordOkOnEnter;

        public MainWindow()
        {
            InitializeComponent();

            var q = new RelayCommand(EnterPasswordMenuItem);

            var z = new RelayCommand(() => SetPassword(this, new RoutedEventArgs()));

            var b = new KeyBinding()
            {
                Command = q,

                Key = Key.P,
                Modifiers = ModifierKeys.Control
            };
            InputBindings.Add(b);


            _changePasswordOkOnEnter = new KeyBinding()
            {
                Command = z,
                Key = Key.Enter,
            };


        }




        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (e.AddedItems == null || e.AddedItems.Count < 1) return;
            //var q = e.AddedItems[0] as MainUnitView;
            var z = sender as TabControl;
            if (z == null) return;
            var q = z.SelectedContent as MainUnitView;

            if (q == null) return;

            q.SetHandlers();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Popup.IsOpen = true;
        }

        private void ClosePopup(object sender, RoutedEventArgs e)
        {
            var but = sender as Control;
            if (but == null) return;

            var s = but.FindLogicalAncestor<Popup>();

            if (s == null) return;
            s.IsOpen = false;
        }

        private void PasswordChanged(object sender, RoutedEventArgs e)
        {
            var z = sender as PasswordBox;
            var y = DataContext as MainViewModel;
            if (z == null || y == null) return;

            y.EnteredPassword(z.Password);

            if (!y.PasswordEnteredOk) return;


            ClosePopup(sender, e);
        }

        private void PasswordChanged1(object sender, RoutedEventArgs e)
        {
            Button1Ok.IsEnabled = PasswordBoxCreate1.Password.Equals(PasswordBoxCreate2.Password);
        }

        private void PasswordChanged2(object sender, RoutedEventArgs e)
        {
            Button1Ok.IsEnabled = PasswordBoxCreate1.Password.Equals(PasswordBoxCreate2.Password);
        }


        private void EnterPasswordMenuItem()
        {
            var y = DataContext as MainViewModel;
            if (y == null) return;
            if (y.PasswordEnteredOk)
            {
                PasswordBox.Clear();
                y.EnteredPassword(string.Empty);
            }
            else
            {
                OpenPasswordBox();
            }
        }

        private void EnterPasswordMenuItem(object sender, RoutedEventArgs e)
        {
            EnterPasswordMenuItem();
        }

        private void SetPassword(object sender, RoutedEventArgs e)
        {
            var y = DataContext as MainViewModel;
            if (y == null) return;

            if (!PasswordBoxCreate1.Password.Equals(PasswordBoxCreate2.Password))
                return;

            y.SetPassword(PasswordBoxCreate1.Password);
            PasswordCreate.IsOpen = false;
            InputBindings.Remove(_changePasswordOkOnEnter);
        }

        /// <summary>
        /// opens popup to enter password
        /// </summary>
        private void OpenPasswordBox()
        {
            Password.IsOpen = true;
            PasswordBox.Focusable = true;
            Keyboard.Focus(PasswordBox);
        }

        private void CreatePasswordBox(object sender, RoutedEventArgs e)
        {
            PasswordBoxCreate1.Clear();
            PasswordBoxCreate2.Clear();
            InputBindings.Add(_changePasswordOkOnEnter);
            PasswordCreate.IsOpen = true;
            PasswordBoxCreate1.Focusable = true;
            Keyboard.Focus(PasswordBoxCreate1);
        }
    }
}
