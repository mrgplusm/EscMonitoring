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
