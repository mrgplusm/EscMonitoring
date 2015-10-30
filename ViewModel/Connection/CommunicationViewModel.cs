using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Common;
using Common.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System.Windows;

namespace Monitoring.ViewModel.Connection
{
    public class CommunicationViewModel : ViewModelBase, ITab
    {
        private readonly MainViewModel _main;

        public readonly ObservableCollection<ConnectionViewModel> OpenConnections =
            new ObservableCollection<ConnectionViewModel>();


#if DEBUG
        public CommunicationViewModel()
        {
            _main = new MainViewModel();
        }
#endif

        public CommunicationViewModel(MainViewModel main)
        {
            _main = main;

            _main.PasswordEntered += _main_PasswordEntered;
            if (!LibraryData.SystemIsOpen) return;
            if (LibraryData.FuturamaSys.Connections == null) return;
            foreach (var c in LibraryData.FuturamaSys.Connections.Select(model => new ConnectionViewModel(model)))
            {
                OpenConnections.Add(c);
                c.Connection.ErrorLineReceived += Connection_ErrorLineReceived;
                c.Connection.ErrorOccured += ConnectionOnErrorOccured;
            }
        }

        private void ConnectionOnErrorOccured(object sender, ErrorEventArgs errorEventArgs)
        {
            Application.Current.Dispatcher.Invoke(OnConnectionError);
        }

        public event EventHandler ConnectionError;

        private void Connection_ErrorLineReceived(object sender, ErrorLineEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => OnDataReceived(e));
        }

        void _main_PasswordEntered(object sender, System.EventArgs e)
        {
            RaisePropertyChanged(() => PasswordEnteredOk);
        }

        public void OpenDetailForCurrentlyConnected()
        {
            foreach (var source in Connections.Where(t => t.ConnectMode == ConnectMode.Install).ToList())
            {
                source.IsInDetailMode = true;
            }
        }

        public event EventHandler<ErrorLineEventArgs> DataReceived;

        


        public int Id => 99;

        public bool PasswordEnteredOk => _main.PasswordEnteredOk;


        public ObservableCollection<ConnectionViewModel> Connections => OpenConnections;


        public ICommand AddConnection
        {
            get
            {
                return new RelayCommand<string>(q =>
                {
                    var z = new ConnectionModel { IsNetConnect = (q == "net") };
                    var vm = new ConnectionViewModel(z);
                    OpenConnections.Add(vm);
                    vm.Connection.ErrorLineReceived += Connection_ErrorLineReceived;
                    if (!LibraryData.SystemIsOpen) return;

                    if (LibraryData.FuturamaSys.Connections == null)
                        LibraryData.FuturamaSys.Connections = new List<ConnectionModel>();
                    LibraryData.FuturamaSys.Connections.Add(z);
                });
            }
        }

        public event EventHandler<ConnectionRemovedEventArgs> ConnectionRemoved;

        protected virtual void OnConnectionRemoved(ConnectionRemovedEventArgs e)
        {            
            ConnectionRemoved?.Invoke(this, e);
        }

        public ICommand RemoveConnection
        {
            get
            {
                return new RelayCommand<ConnectionViewModel>(s =>
                    {
                        s.Connection.ErrorLineReceived -= Connection_ErrorLineReceived;
                        s.Connection.ErrorOccured -= ConnectionOnErrorOccured;
                        s.EndConnection();

                        OpenConnections.Remove(s);

                        if (!LibraryData.SystemIsOpen) return;
                        if (LibraryData.FuturamaSys.Connections == null)
                        {
                            LibraryData.FuturamaSys.Connections = new List<ConnectionModel>();
                            return;
                        }
                        LibraryData.FuturamaSys.Connections.Remove(s.DataModel);
                        OnConnectionRemoved(new ConnectionRemovedEventArgs() { Connection = s.Connection });
                    });
            }
        }

        protected virtual void OnDataReceived(ErrorLineEventArgs e)
        {
            DataReceived?.Invoke(this, e);
        }

        protected virtual void OnConnectionError()
        {
            ConnectionError?.Invoke(this, EventArgs.Empty);
        }
    }

    public class ConnectionRemovedEventArgs : EventArgs
    {
        public Common.Connection Connection { get; set; }
    }
}