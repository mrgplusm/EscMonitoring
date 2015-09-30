using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Mime;
using System.Windows;
using Common;
using Common.Model;
using GalaSoft.MvvmLight;

namespace Monitoring.ViewModel.Connection
{
    public class ConnectionViewModel : ViewModelBase, IEquatable<ConnectionViewModel>
    {
        private string _errorInfo;

        public ConnectionViewModel(ConnectionModel cm)
        {
            DataModel = cm;
            Connection = new Common.Connection();


            Connection.ConnectModeChanged += (conn, mode) => Application.Current.Dispatcher.Invoke(() =>
            {
                ErrorInfo = string.Empty;
                RaisePropertyChanged(() => ConnectMode);
            });
        
            Connection.ErrorLineReceived += (sender, model) => Application.Current.Dispatcher.Invoke(()=>  MainViewModel.ErrorLineReceived(model));
            Connection.ErrorOccured += ConnectionOnErrorOccured;

            Connection.UnitIdChanged += delegate { RaisePropertyChanged(() => UnitId); };
        }

        private void ConnectionOnErrorOccured(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {

                if (DataModel.Errors == null) DataModel.Errors = new List<string>();
                DataModel.Errors.Add(DateTime.Now.ToString("u") + '\t' + message);
                ErrorInfo = message;
                SendEmailViewModel.SendEmail();
            });
        }

        public Common.Connection Connection { get; private set; }

        public ConnectionModel DataModel { get; private set; }


        public int DesiredUnitId { get; set; }


        public bool IsNetwork
        {
            get { return DataModel.IsNetConnect; }
        }

        public ConnectType ConnectType
        {
            get { return DataModel.IsNetConnect ? ConnectType.Ethernet : ConnectType.USB; }
        }

        public int UnitId
        {
            get { return Connection.UnitId; }
        }


        private ObservableCollection<string> _ports;
        public ObservableCollection<string> Ports
        {
            get
            {
                return _ports ?? (_ports = new ObservableCollection<string>(PortList.GetList()));
            }
        }


        public string ErrorInfo
        {
            get
            {
#if DEBUG
                if (IsInDesignMode)
                {
                    return "This is an errorThis is an errorThis is an errorThis is an errorThis is " +
                           "an errorThis is an errorThis is an errorThis is an errorThis is an error";
                }
#endif
                return _errorInfo;
            }
            private set
            {
                _errorInfo = value;
                RaisePropertyChanged(() => ErrorInfo);
            }
        }

        public ConnectMode ConnectMode
        {
            get
            {
                return Connection.Mode;
            }
        }

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

        public override bool Equals(object other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.GetType() == GetType() && Equals((ConnectionViewModel)other);
        }

        public override int GetHashCode()
        {
            return (DataModel != null ? DataModel.GetHashCode() : 0);
        }

        public bool Equals(ConnectionViewModel other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(DataModel, other.DataModel);
        }
    }
}