using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Common;
using Common.Model;
using CsvHelper;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using Monitoring.Properties;
using Monitoring.View;
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
                    LibraryData.FuturamaSys?.ProgramEvents?.Add(new ProgramEvent(false, DateTime.Now));
                    Save();
                    Settings.Default.Save();
                    CloseAllConnections();
                };
            }

            ErrorList = new ObservableCollection<ErrorLineViewModel>();

            LoadStoredErrors();
            _saveFileTimer = new Timer { AutoReset = true, Enabled = true, Interval = 300000 };
            _saveFileTimer.Elapsed += SaveFileTimerElapsed;



#if DEBUG
            if (IsInDesignModeStatic)
            {
                ErrorList = new ObservableCollection<ErrorLineViewModel>();
                for (var i = 0; i < 10; i++)
                {
                    ErrorList.Add(new ErrorLineViewModel(new DeviceError(

                        0x01,
                        SyMo.Fds,
                        3,
                        new byte[] { 0x01, 0x02 },
                        ErDt.AmpDefect
                    )
                        , ErrorStatuses.FaultSet, 3, DateTime.Now, new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 }));
                }
            }
#endif
            if (!IsInDesignMode)
            {
                OpenFileIfDefined();
            }
        }

        private void _notificationWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OnErrorNotificationClicked();
            Application.Current.MainWindow.Activate();
        }

        public event EventHandler ErrorNotificationClicked;

        private NotificationWindow _notificationWindow;
        private void ShowNotificationWindow(ErrorLineViewModel error)
        {
            _notificationWindow?.Close();
            _notificationWindow = new NotificationWindow { ErrorLine = error };
            _notificationWindow.MouseDown += _notificationWindow_MouseDown;
            _notificationWindow.Show();
        }


        private void CloseAllConnections()
        {
            if (CommunicationView == null) return;
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


        public SchematicOverView Schematic => _schematic ?? (_schematic = new SchematicOverView(this));

        private void SetFileProperties()
        {
            if (!LibraryData.SystemIsOpen) return;

            LoadStoredErrors();

            RaisePropertyChanged(() => ErrorList);

            MainUnitViewModels.AddRange(LibraryData.FuturamaSys.MainUnits.Select(n => new MainUnitViewModel(n, this)));

            Tabs.Add(Schematic);
            Tabs.Add(CommunicationView);
            Tabs.Add(Email);

            foreach (var mainUnit in MainUnitViewModels)
            {
                Tabs.Add(mainUnit);
            }

            ConnectAll();

        }

        private CommunicationViewModel _communicationView;
        public CommunicationViewModel CommunicationView
        {
            get
            {
                if (_communicationView != null) return _communicationView;
                var ret = new CommunicationViewModel(this);

                _communicationView = ret;
                _communicationView.DataReceived += (o, e) => ErrorLineReceived(e.Model);
                _communicationView.ConnectionError += (sender, args) => SelectedTabItem = CommunicationView;
                return ret;
            }
        }

        private void ConnectAll()
        {
            foreach (var connection in CommunicationView.Connections)
            {
                connection.Connect();
            }
        }

        public readonly List<MainUnitViewModel> MainUnitViewModels = new List<MainUnitViewModel>();

        private void DetachErrorHandlers()
        {
            MainUnitViewModels.Clear();
            ErrorList.Clear();

            Tabs.Clear();
        }


        public static event EventHandler<ErrorLineViewModel> Error;

        private static void OnError(ErrorLineViewModel e)
        {
            EventHandler<ErrorLineViewModel> handler = Error;
            handler?.Invoke(null, e);
        }



        public void ErrorLineReceived(ErrorLineModel error)
        {
            if (!LibraryData.SystemIsOpen) return;
            if (LibraryData.FuturamaSys.Errors == null) return;
            error.IsVisible = true;
            LibraryData.FuturamaSys.Errors.Add(error);
            
            var z = new ErrorLineViewModel(error);
            ErrorList.Add(z);
            OnError(z);
            SwitchTabForError(error);
            ShowNotificationWindow(z);
            var s = new EmailSender(LibraryData.FuturamaSys.Email);
            s.SendEmail();
        }

        public ITab SelectedTabItem
        {
            get
            {
                return _selectedTabItem ?? Schematic;
            }
            set
            {
                _selectedTabItem = value;
                RaisePropertyChanged(() => SelectedTabItem);
            }
        }

        private void SwitchTabForError(ErrorLineModel error)
        {
            var chooseTabItem = Tabs.FirstOrDefault(i => i.Id == error.EscUnit);
            if (chooseTabItem == null) return;
            SelectedTabItem = chooseTabItem;
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
            handler?.Invoke(this, EventArgs.Empty);
        }


        public void AddToMainScreen(DiagramObject system)
        {
            Schematic.AddToSchematic(system);
        }


        public void RemoveFromMainScreen(DiagramObject system)
        {
            Schematic.RemoveFromSchematic(system, EventArgs.Empty);
        }

        public ICommand OpenFile => new RelayCommand(Open);


        public event EventHandler PasswordEntered;

        protected virtual void OnPasswordEntered()
        {
            var handler = PasswordEntered;
            handler?.Invoke(this, EventArgs.Empty);
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

            if (!string.IsNullOrWhiteSpace(Settings.Default.RecentLocationProject))
                dlg.InitialDirectory = Settings.Default.RecentLocationProject;

            var q = dlg.ShowDialog();
            if (!q.HasValue || !q.Value || string.IsNullOrWhiteSpace(dlg.FileName)) return;



            if (string.IsNullOrEmpty(dlg.FileName))
            {
                Application.Current.Shutdown();
            }

            Open(dlg.FileName);
        }

        private void Open(string fileName)
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
            SetFileProperties();
            RaisePropertyChanged(() => InstallerVersion);
            LibraryData.FuturamaSys?.ProgramEvents?.Add(new ProgramEvent(true, DateTime.Now));

        }


        public EventHandler FileChanged;

        private void OnFileChanged()
        {
            var v = FileChanged;
            v?.Invoke(this, EventArgs.Empty);
        }

        public ICommand ClearList
        {
            get
            {

                return new RelayCommand(() =>
                {
                    if (LibraryData.FuturamaSys.ClearedErrors == null)
                        LibraryData.FuturamaSys.ClearedErrors = new List<LogClearEntry>();

                    var clearid = LibraryData.FuturamaSys.ClearedErrors.Count;
                    LibraryData.FuturamaSys.ClearedErrors.Add(new LogClearEntry()
                    {
                        LogCleared = DateTime.Now,
                        LogClearedBy =
                        LogClearedBy,
                        Id = clearid
                    });

                    //send an email before clearing the list
                    var s = new EmailSender(LibraryData.FuturamaSys.Email);
                    s.SendEmail();

                    foreach (var error in ErrorList)
                    {
                        error.DataModel.ClearedById = clearid;
                        error.DataModel.IsVisible = false;
                    }

                    ErrorList.Clear();
                    LogClearedBy = string.Empty;
                    OnLogCleared();

                }, () => LibraryData.SystemIsOpen && !string.IsNullOrWhiteSpace(LogClearedBy));
            }
        }

        private void OnLogCleared()
        {
            var lc = LogCleared;
            lc?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler LogCleared;

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

        private SendEmailViewModel _email;
        public SendEmailViewModel Email => _email ?? (_email = new SendEmailViewModel());

        public bool IsConnected
        {
            get
            {
                return CommunicationView.OpenConnections.All(n => (int)n.ConnectMode > 0)
                    && CommunicationView.OpenConnections.Count > 0;
            }
        }

        public string AppVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

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

        public string NotConnectedMcu => Main.NotConnectedMcu;

        public string ConnectedMcu => Main.ConnectedMcu;

        public ICommand CsvWrite => new RelayCommand(WriteToCsv);

        public ICommand CsvWriteEvents => new RelayCommand(WriteToCsvEvents);

        public void WriteToCsv()
        {
            var fdlg = CsvFileDialogFactory();
            
            fdlg.FileOk += (sender, args) =>
            {
                if (!fdlg.CheckPathExists) return;
                if (!LibraryData.SystemIsOpen) return;
                if (LibraryData.FuturamaSys.Errors == null) return;

                using (var file =
                    new StreamWriter(fdlg.FileName))
                {
                    var csv = new CsvWriter(file);
                    csv.WriteRecords(LibraryData.FuturamaSys.Errors.Select(n => new ErrorLineCsv(n)));
                }
            };

            Application.Current.Dispatcher.Invoke(() => fdlg.ShowDialog());
        }


        private static FileDialog CsvFileDialogFactory()
        {
            FileDialog f = new SaveFileDialog();
            f.AddExtension = true;
            f.DefaultExt = ".csv";
            f.Filter = "Comma-separated values (.csv) | *.csv";
            return f;
        }

        public void WriteToCsvEvents()
        {

            var fdlg = CsvFileDialogFactory();
            fdlg.FileOk += (sender, args) =>
            {
                if (!fdlg.CheckPathExists) return;
                if (!LibraryData.SystemIsOpen) return;
                if (LibraryData.FuturamaSys.ProgramEvents == null) return;
                using (var file =
                    new StreamWriter(fdlg.FileName))
                {
                    var csv = new CsvWriter(file);
                    csv.WriteRecords(LibraryData.FuturamaSys.ProgramEvents);
                }
            };                        

            Application.Current.Dispatcher.Invoke(() => fdlg.ShowDialog());
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


        private ErrorLogViewModel _errorlog;
        public ErrorLogViewModel ErrorLog => _errorlog ?? (_errorlog = new ErrorLogViewModel());

        private void LoadStoredErrors()
        {
            if (LibraryData.FuturamaSys == null) return;
            ErrorList.Clear();

            if (LibraryData.FuturamaSys.Errors == null || LibraryData.FuturamaSys.Errors.Count == 0) return;
            foreach (var error in LibraryData.FuturamaSys.Errors.Where(i => i.IsVisible).OrderByDescending(q => q.Id))
            {
                ErrorList.Add(new ErrorLineViewModel(error));
            }
        }

        public ObservableCollection<ErrorLineViewModel> ErrorList { get; private set; }



        private void Close()
        {
            if (!PasswordEnteredOk) return;
            Save();
            LibraryData.SystemFileName = string.Empty;
            LibraryData.CloseProject();
            RaisePropertyChanged(() => FileName);
            RaisePropertyChanged(() => InstallerVersion);

            DetachErrorHandlers();

            LibraryData.SystemFileName = string.Empty;
            _email = null;

            RaisePropertyChanged(() => FileName);
            RaisePropertyChanged(() => InstallerVersion);
        }

        private readonly Timer _saveFileTimer;
        private string _logClearedBy;
        private byte[] _enteredPassword;
        private static readonly object SaveLock = new object();

        private void Save()
        {
            if (!LibraryData.SystemIsOpen) return;
            lock (SaveLock)
            {
                _saveFileTimer.Stop();

                try
                {
                    FileManagement.SaveSystemFile(LibraryData.FuturamaSys, LibraryData.SystemFileName);
                }
                catch (Exception e)
                {

                    MessageBox.Show(string.Format(Main.MessageBoxSaveFileText, e.Message), Main.MessageBoxSaveFileHeader,
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
                finally
                {
                    _saveFileTimer.Start();
                }
            }
        }

        private ObservableCollection<ITab> _tabs;
        public ObservableCollection<ITab> Tabs => _tabs ?? (_tabs = new ObservableCollection<ITab>());

        private ListCollectionView _tabCollectionView;
        public ListCollectionView TabListView
        {
            get
            {
                if (_tabCollectionView == null)
                {
                    _tabCollectionView = (ListCollectionView)CollectionViewSource.GetDefaultView(Tabs);
                    _tabCollectionView.CustomSort = new TabSorter();
                    _tabCollectionView.IsLiveSorting = true;

                }
                return _tabCollectionView;
            }
        }

        private ListCollectionView _errorList;
        private SchematicOverView _schematic;
        private ITab _selectedTabItem;

        public ListCollectionView ErrorListView
        {
            get
            {
                if (_errorList == null)
                {
                    _errorList = (ListCollectionView)CollectionViewSource.GetDefaultView(ErrorList);

                    _errorList.GroupDescriptions?.Add(new PropertyGroupDescription() { PropertyName = "Grouping" });

                    _errorList.CustomSort = new ErrorLineSorter();
                    _errorList.IsLiveFiltering = true;
                    _errorList.IsLiveGrouping = true;
                }
                return _errorList;
            }
        }


        protected virtual void OnErrorNotificationClicked()
        {
            ErrorNotificationClicked?.Invoke(this, EventArgs.Empty);
        }
    }


    class TabSorter : IComparer
    {
        public int Compare(object x, object y)
        {
            var X = x as ITab;
            var Y = y as ITab;
            return X.Id.CompareTo(Y.Id);
        }
    }

    class ErrorLineSorter : IComparer
    {
        public int Compare(object x, object y)
        {
            var X = x as ErrorLineViewModel;
            var Y = y as ErrorLineViewModel;
            return Y.Id.CompareTo(X.Id);
        }
    }    

    public class ErrorLineCsv
    {
        private readonly ErrorLineModel _error;

        public ErrorLineCsv(ErrorLineModel error)
        {
            _error = error;
        }

        public string EscUnit => _error.EscUnit.ToString();

        public string EscData => GenericMethods.Tostring(_error.EscData);


        public string Id => _error.Id.ToString();

        public string DateTime => _error.Date.ToString();

        public string Module => _error.Device?.Module.ToString();

        public string Detail => _error.Device?.Detail.ToString();

        public string Number => _error.Device?.Number.ToString();


        public string EmailSend => _error.EmailSend.ToString();


        public string IsVisible => _error.IsVisible.ToString();


        public string ClearedById => _error.ClearedById.ToString();
    }


}
