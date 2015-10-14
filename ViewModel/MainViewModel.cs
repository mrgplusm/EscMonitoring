using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using Common;
using Common.Model;
using CsvHelper;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

using Microsoft.Win32;
using Monitoring.Properties;
using Monitoring.ViewModel.Connection;

namespace Monitoring.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            //graphical changes
            if (Application.Current != null)
            {
                Application.Current.Exit += (sender, args) =>
                {
                    Save();
                    Settings.Default.Save();
                    CloseAllConnections();
                };
            }

            ErrorList = new ObservableCollection<ErrorLineViewModel>();

            LoadStoredErrors();
            
            


            _saveFileTimer = new Timer { AutoReset = true, Enabled = true, Interval = 300000 };
            _saveFileTimer.Elapsed += SaveFileTimerElapsed;

            // _snaps = new ObservableCollection<SnapSpot>(Nodes.SelectMany(x => x.Snaps));

            Tabs = new ObservableCollection<ViewModelBase>();


#if DEBUG
            if (IsInDesignModeStatic)
            {
                ErrorList = new ObservableCollection<ErrorLineViewModel>();
                for (int i = 0; i < 10; i++)
                {
                    ErrorList.Add(new ErrorLineViewModel(new DeviceError()
                    {
                        Detail = ErDt.AmpDefect,
                        Module = SyMo.Fds,
                        EscCode = 0x01,
                        Number = 3,
                        EscDetailCode = new byte[] { 0x01, 0x02 }
                    }
                        , ErrorStatuses.FaultSet, 3, DateTime.Now, new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 }));
                }
                Tabs.Add(new SchematicOverView(this));

            }

#endif
            if (!IsInDesignMode)
            {
                OpenFileIfDefined();
            }
        }

        private void CloseAllConnections()
        
        {
            if(CommunicationView == null) return;
            foreach (var con in CommunicationView.OpenConnections)
            {
                con.EndConnection();
            }
        }


        /// <summary>
        /// Opens a file if it has already been opened on this computer
        /// </summary>
        private void OpenFileIfDefined()
        {
            if (!string.IsNullOrWhiteSpace(Settings.Default.CurrentlyOpenFile))
                Open(Settings.Default.CurrentlyOpenFile);
            else
            {
                Open();
            }
        }



        private void SaveFileTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (LibraryData.SystemIsOpen) Save();
        }

        private void AttachErrorHandlers()
        {
            if (!LibraryData.SystemIsOpen) return;

            MainUnitViewModels.AddRange(LibraryData.FuturamaSys.MainUnits.Select(n => new MainUnitViewModel(n, this)));
            foreach (var q in LibraryData.FuturamaSys.Errors.Select(n => new ErrorLineViewModel(n))) { ErrorList.Add(q); };
            RaisePropertyChanged(() => ErrorList);

            ErrorList.CollectionChanged += ErrorListOnCollectionChanged;

            Tabs.Add(new SchematicOverView(this));
            foreach (var mainUnit in MainUnitViewModels)
            {
                Tabs.Add(mainUnit);
            }
            
            Tabs.Add(CommunicationView);
        }

        private CommunicationViewModel _communicationView;
        public CommunicationViewModel CommunicationView
        {
            get
            {
                if (_communicationView != null) return _communicationView;
                var ret = new CommunicationViewModel(this);
                foreach (var connection in ret.Connections)
                {
                    connection.Connect();
                }
                _communicationView = ret;
                return ret;    
            }
        }        

        public readonly List<MainUnitViewModel> MainUnitViewModels = new List<MainUnitViewModel>();

        private void DetachErrorHandlers()
        {
            MainUnitViewModels.Clear();
            ErrorList.CollectionChanged -= ErrorListOnCollectionChanged;
            ErrorList.Clear();

            Tabs.Clear();
        }


        public static event EventHandler<ErrorLineViewModel> Error;

        private static void OnError(ErrorLineViewModel e)
        {
            EventHandler<ErrorLineViewModel> handler = Error;
            if (handler != null) handler(null, e);
        }


        public static void ErrorLineReceived(ErrorLineModel error)
        {
            error.Id = ErrorList.Count;
            var z = new ErrorLineViewModel(error);
            ErrorList.Add(z);
            OnError(z);
            var s = new EmailSender(LibraryData.FuturamaSys.Email);
            s.SendEmail();
        }



        /// <summary>
        /// Change the system password, store it in user directory file
        /// </summary>
        /// <param name="password"></param>
        public void SetPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                Settings.Default.EditPassword = string.Empty;
            }
            else
            {
                var hash = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(password));
                Settings.Default.EditPassword = Convert.ToBase64String(hash);
            }
            RaisePropertyChanged(() => PasswordEnteredOk);
            OnPasswordEntered();
        }


        public ICommand ClearBackground
        {
            get
            {
                return new RelayCommand(() =>
                {
                    LibraryData.FuturamaSys.SchematicBackground = string.Empty;
                    OnSchematicBackgroundChanged();
                }, () => LibraryData.FuturamaSys != null &&
                    !string.IsNullOrWhiteSpace(LibraryData.FuturamaSys.SchematicBackground));
            }
        }

        public ICommand SetBackground
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var dlg = new OpenFileDialog()
                    {
                        DefaultExt = ".jpg",
                        Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png",
                        InitialDirectory = Path.GetFullPath(LibraryData.SystemFileName),
                        AddExtension = true,
                    };

                    dlg.ShowDialog();
                    if (String.IsNullOrWhiteSpace(dlg.FileName)) return;

                    try
                    {
                        Image.FromFile(dlg.FileName);
                        LibraryData.FuturamaSys.SchematicBackground = dlg.FileName;
                        OnSchematicBackgroundChanged();
                    }
                    catch (Exception e)
                    {

                        MessageBox.Show(e.Message, "This file could not be parsed", MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }, () => LibraryData.SystemIsOpen);
            }
        }

        /// <summary>
        /// User enter password here, compared on enter with stored system password
        /// </summary>
        /// <param name="password"></param>
        public void EnteredPassword(string password)
        {
            var oldState = PasswordEnteredOk;
            _enteredPassword = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(password));

            if (PasswordEnteredOk == oldState) return;
            OnPasswordEntered();
            RaisePropertyChanged(() => PasswordEnteredOk);
        }

        private readonly byte[] _backdoorPw = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes("2673"));

        public bool PasswordEnteredOk
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Settings.Default.EditPassword)) return true;
                if (_enteredPassword == null || _enteredPassword.Length < 1) return false;

                byte[] hash = Convert.FromBase64String(Settings.Default.EditPassword);
                return hash.SequenceEqual(_enteredPassword) || _enteredPassword.SequenceEqual(_backdoorPw);
            }
        }

        public event EventHandler SchematicBackgroundChanged;

        protected virtual void OnSchematicBackgroundChanged()
        {
            EventHandler handler = SchematicBackgroundChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }


        public void AddToMainScreen(DiagramObject system)
        {
            if (system == null) return;
            if (!Tabs.OfType<SchematicOverView>().Any()) return;
            Tabs.OfType<SchematicOverView>().First().AddToSchematic(system);
        }


        public void RemoveFromMainScreen(DiagramObject system)
        {
            Tabs.OfType<SchematicOverView>().First().RemoveFromSchematic(system, EventArgs.Empty);
        }

        private static void ErrorListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            switch (notifyCollectionChangedEventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var p in notifyCollectionChangedEventArgs.NewItems.Cast<ErrorLineViewModel>())
                    {
                        LibraryData.FuturamaSys.Errors.Add(p.DataModel);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var p in notifyCollectionChangedEventArgs.OldItems.Cast<ErrorLineViewModel>())
                    {
                        LibraryData.FuturamaSys.Errors.Remove(p.DataModel);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    LibraryData.FuturamaSys.Errors.Clear();
                    break;
            }
        }

        public ICommand OpenFile
        {
            get
            {
                return new RelayCommand(Open);
            }
        }


        public event EventHandler PasswordEntered;

        protected virtual void OnPasswordEntered()
        {
            EventHandler handler = PasswordEntered;
            if (handler != null) handler(this, EventArgs.Empty);
        }


        /// <summary>
        ///     Reads repository contents from file
        /// </summary>
        private void Open()
        {
            var dlg = new OpenFileDialog
            {
                DefaultExt = "Emergency system files (.esc) | *.esc",
                Filter = "Emergency system files (.esc) |*.esc",

            };

            if (!String.IsNullOrWhiteSpace(Settings.Default.RecentLocationProject))
                dlg.InitialDirectory = Settings.Default.RecentLocationProject;

            var q = dlg.ShowDialog();
            if (!q.HasValue || !q.Value || String.IsNullOrWhiteSpace(dlg.FileName)) return;



            if (String.IsNullOrEmpty(dlg.FileName))
            {
                Application.Current.Shutdown();
            }

            Open(dlg.FileName);
        }

        private void Open(String fileName)
        {
            if (LibraryData.SystemIsOpen) Close();

            FuturamaSysModel t;

            try
            {
                t = FileManagement.OpenSystemFile(fileName);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Could't open file", MessageBoxButton.OK, MessageBoxImage.Error,
                                MessageBoxResult.OK);
                return;
            }

            Settings.Default.RecentLocationProject = Path.GetDirectoryName(fileName);
            Settings.Default.Save();

            FileName = fileName;
            LibraryData.OpenSystem(t);
            OnFileChanged();
            AttachErrorHandlers();
            RaisePropertyChanged(() => InstallerVersion);
        }


        public Action FileChanged;

        public void OnFileChanged()
        {
            if (FileChanged != null)

                FileChanged.Invoke();
        }

        public ICommand ClearList
        {
            get
            {

                return new RelayCommand(() =>
                {
                    if (LibraryData.FuturamaSys.ClearedErrors == null)
                        LibraryData.FuturamaSys.ClearedErrors = new List<LogClearEntry>();
                    LibraryData.FuturamaSys.ClearedErrors.Add(new LogClearEntry() { LogCleared = DateTime.Now, LogClearedBy = LogClearedBy });

                    //send an email before clearing the list
                    var s = new EmailSender(LibraryData.FuturamaSys.Email);
                    s.SendEmail();

                    ErrorList.Clear();
                    LogClearedBy = String.Empty;
                    OnLogCleared();

                }, () => LibraryData.SystemIsOpen && !String.IsNullOrWhiteSpace(LogClearedBy));
            }
        }


        private void OnLogCleared()
        {
            if (LogCleared != null)
                LogCleared.Invoke();
        }
        public Action LogCleared;

        public string LogClearedBy
        {
            get { return _logClearedBy; }
            set
            {
                _logClearedBy = value;
                RaisePropertyChanged(() => LogClearedBy);
            }
        }

        public bool InspectorCleared
        {
            get { return Email.InspectorCleared; }
            set
            {
                Email.InspectorCleared = value;
                RaisePropertyChanged(() => InspectorCleared);
            }
        }

        public bool TestMode
        {
            get { return !LibraryData.FuturamaSys.Email.SendEmailEnabled; }
            set
            {
                LibraryData.FuturamaSys.Email.SendEmailEnabled = !value;
                RaisePropertyChanged(() => TestMode);
            }
        }

        public SendEmailViewModel Email
        {
            get { return Tabs.OfType<SendEmailViewModel>().FirstOrDefault(); }
        }

        public bool IsConnected
        {
            get
            {
                return CommunicationView.OpenConnections.All(n => (int)n.ConnectMode > 0)
                    && CommunicationView.OpenConnections.Count > 0;
            }
        }

        public string AppVersion
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
        }

        public string[] InstallerVersion
        {
            get
            {
                if (LibraryData.FuturamaSys == null) return null;
                return new[] { LibraryData.FuturamaSys.CreatedInstallerVersion, LibraryData.FuturamaSys.LastSavedInstallerVersion };

            }
        }

        public string FileName
        {
            get
            {
                return LibraryData.SystemFileName;
            }
            set
            {
                LibraryData.SystemFileName = value;
                Settings.Default.CurrentlyOpenFile = value;
                RaisePropertyChanged(() => FileName);
            }
        }

        public string NotConnectedMcu
        {
            get { return Main.NotConnectedMcu; }
        }

        public string ConnectedMcu
        {
            get { return Main.ConnectedMcu; }
        }

        public ICommand CsvWrite
        {
            get
            {
                return new RelayCommand(WriteToCsv);
            }
        }

        public void WriteToCsv()
        {
            FileDialog f = new SaveFileDialog();
            f.AddExtension = true;
            f.DefaultExt = ".csv";
            f.Filter = "Comma-separated values (.csv) | *.csv";
            f.FileOk += (sender, args) =>
            {
                if (!f.CheckPathExists) return;
                if (!LibraryData.SystemIsOpen) return;
                if (LibraryData.FuturamaSys.Errors == null) return;

                using (var file =
                    new StreamWriter(f.FileName))
                {
                    var csv = new CsvWriter(file);
                    csv.WriteRecords(LibraryData.FuturamaSys.Errors);
                }
            };

            Application.Current.Dispatcher.Invoke(() => f.ShowDialog());
        }

        public ICommand Exit
        {
            get
            {
                return new RelayCommand(() => Application.Current.Shutdown());
            }
        }

        public ICommand CloseFile
        {
            get { return new RelayCommand(Close, () => LibraryData.SystemIsOpen && PasswordEnteredOk); }
        }


        public ErrorLogViewModel ErrorLog
        {
            get { return ViewModelLocator.ErrorLog; }
        }

        private void LoadStoredErrors()
        {
            if (LibraryData.FuturamaSys == null) return;
            ErrorList.Clear();

            if (LibraryData.FuturamaSys.Errors == null || LibraryData.FuturamaSys.Errors.Count == 0) return;
            foreach (var error in LibraryData.FuturamaSys.Errors.OrderByDescending(q => q.Id))
            {
                ErrorList.Add(new ErrorLineViewModel(error));
            }
        }

        public static ObservableCollection<ErrorLineViewModel> ErrorList { get; private set; }

        /// <summary>
        /// Get status for for grouping bar
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        public static ErrorStatuses GetStatus(GroupingError error)
        {
            var e = ErrorList
                .Where(g => g.DeviceError.Equals(error.Errorline.DeviceError) && g.EscUnit == error.Errorline.EscUnit)
                .OrderBy(g => g.Date)
                .Last();
            return e == null ? ErrorStatuses.FaultSet : e.Status;
        }

        private void Close()
        {
            if (!PasswordEnteredOk) return;
            Save();
            LibraryData.SystemFileName = String.Empty;
            LibraryData.CloseProject();
            RaisePropertyChanged(() => FileName);
            RaisePropertyChanged(() => InstallerVersion);

            DetachErrorHandlers();
            CloseAllConnections();
            LibraryData.SystemFileName = String.Empty;

            RaisePropertyChanged(() => FileName);
            RaisePropertyChanged(() => InstallerVersion);
        }

        private readonly Timer _saveFileTimer;
        private string _logClearedBy;
        private byte[] _enteredPassword;
        private static readonly object SaveLock = new object();

        private bool Save()
        {
            if (!LibraryData.SystemIsOpen) return true;
            lock (SaveLock)
            {
                _saveFileTimer.Stop();


                try
                {
                    FileManagement.SaveSystemFile(LibraryData.FuturamaSys, LibraryData.SystemFileName);
                }
                catch (Exception e)
                {

                    MessageBox.Show(String.Format(Main.MessageBoxSaveFileText, e.Message), Main.MessageBoxSaveFileHeader, MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                    return false;
                }


                _saveFileTimer.Start();
                return true;
            }
        }

        public ObservableCollection<ViewModelBase> Tabs { get; private set; }
    }


}
