using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using Common;
using Common.Model;
using GalaSoft.MvvmLight;
using Timer = System.Timers.Timer;

namespace Monitoring.ViewModel.Connection
{
    public class ConnectionViewModel : ViewModelBase
    {
        private string _errorInfo;

        private const int Timeout = 3600;
        private static readonly Timer ConnectionEmailTimer = new Timer() {AutoReset = true, Enabled = true, Interval = Timeout};

        /// <summary>
        /// Used to not spam when a connection breaks. True: can be send. False: Wait for timer to reset.
        /// </summary>
        private static volatile bool _blockEmailSending;

#if DEBUG
        public ConnectionViewModel()
        {
            DataModel = new ConnectionModel();
            Connection = new Common.Connection();
            ErrorInfo = "This is an error";
        }
#endif

        static ConnectionViewModel()
        {
            ConnectionEmailTimer.Elapsed += (sender, args) => _blockEmailSending = true;
        }

        private static void ResetBlockingTimer()
        {
            ConnectionEmailTimer.Stop();
            ConnectionEmailTimer.Start();
        }

        public ConnectionViewModel(ConnectionModel cm)
        {
            DataModel = cm;
            Connection = new Common.Connection();


            Connection.ConnectModeChanged += (conn, mode) =>
            {
                _blockEmailSending = false;
                if (Application.Current == null) return;
                Application.Current.Dispatcher.Invoke(() =>
            {
                ErrorInfo = string.Empty;
                RaisePropertyChanged(() => ConnectMode);
                
            });
                
            };


            Connection.ErrorOccured += ConnectionOnErrorOccured;
            Connection.UnitIdChanged += delegate
            {
                RaisePropertyChanged(() => UnitId);
                _blockEmailSending = false;
            };
        }

        private void ConnectionOnErrorOccured(object sender, ErrorEventArgs errorEventArgs)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    StartReconnecTimer();
                    if (DataModel.Errors == null) DataModel.Errors = new List<string>();
                    DataModel.Errors.Add($"{DateTime.Now.ToString("u")}{'\t'}{errorEventArgs.Exception}");
                    ErrorInfo = errorEventArgs.Exception.ToString();

                    if (_blockEmailSending) return;
                    var s = new EmailSender(LibraryData.FuturamaSys.Email);
                    s.SendEmail();
                    ResetBlockingTimer();
                    _blockEmailSending = true;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            });
        }

        private volatile bool _reconnectTimerRunning;
        private readonly object _reconnectTimerlock = new object();

        private void StartReconnecTimer()
        {
            Task.Factory.StartNew(() =>
            {
                lock (_reconnectTimerlock)
                {
                    if (_reconnectTimerRunning) return;
                    var ret = new Timer { AutoReset = false, Enabled = true, Interval = 62 * 1000 };
                    _reconnectTimerRunning = true;
                    ret.Elapsed += (o, e) =>
                    {
                        lock (_reconnectTimerlock)
                        {
                            Connect();
                            ret.Dispose();
                            _reconnectTimerRunning = false;
                        }
                    };
                }
            });
        }

        public void Connect()
        {
            Connection.CreateConnection(DataModel.IpAddress, ConnectType, ConnectMode.Monitoring);
        }

        public Common.Connection Connection { get; }

        public ConnectionModel DataModel { get; }


        public int DesiredUnitId { get; set; }


        public bool IsNetwork => DataModel.IsNetConnect;

        public ConnectType ConnectType => DataModel.IsNetConnect ? ConnectType.Ethernet : ConnectType.USB;

        public int UnitId => Connection.UnitId;


        private ObservableCollection<string> _ports;
        public ObservableCollection<string> Ports => _ports ?? (_ports = new ObservableCollection<string>(PortList.GetList()));


        public string ErrorInfo
        {
            get
            {
                return _errorInfo;
            }
            private set
            {
                _errorInfo = value;
                RaisePropertyChanged(() => ErrorInfo);
            }
        }

        public ConnectMode ConnectMode => Connection.Mode;

        public bool ConnectionButtonIsEnabled { get; set; }


        public bool IsInDetailMode
        {
            get { return DataModel.IsInDetailMode; }
            set
            {
                DataModel.IsInDetailMode = value;
                RaisePropertyChanged(() => IsInDetailMode);
            }
        }

        public string Ipaddress
        {
            get { return DataModel.IpAddress; }
            set
            {
                DataModel.IpAddress = value;
                RaisePropertyChanged(() => Ipaddress);
            }
        }

        public void EndConnection()
        {
            ErrorInfo = string.Empty;
            Connection.Disconnect();
        }

    }
}