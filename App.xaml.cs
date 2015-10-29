using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using Monitoring.Email;

namespace Monitoring
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex _m;

        private App()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
#if !DEBUG
            if (IsSingleInstance())
            {
                //OpenRegKey();
                return;
            }
            MessageBox.Show("The installer/monitor software is already running", "Allready open",
                            MessageBoxButton.OK, MessageBoxImage.Error);
            Current.Shutdown();
#endif
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {

            MessageBox.Show(e.ExceptionObject.ToString());

        }

        private const string MutexName = "futuramaMonitoring";

        private static bool IsSingleInstance()
        {

            // Try to open existing mutex.
            Mutex ret;
            if (Mutex.TryOpenExisting(MutexName, out ret))
                return false;

            _m = new Mutex(true, MutexName);

            // Only one instance.
            return true;
        }
    }
}
