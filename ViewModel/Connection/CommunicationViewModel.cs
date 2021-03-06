﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Common;
using Common.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace Monitoring.ViewModel.Connection
{
    public interface ITabControl
    {
        int Id { get; }
    }

    public class CommunicationViewModel : ViewModelBase, ITabControl
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
            }
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


        public int Id
        {
            get { return 99; }
        }

        public bool PasswordEnteredOk
        {
            get { return _main.PasswordEnteredOk; }
        }


        public ObservableCollection<ConnectionViewModel> Connections
        {
            get
            {
                return OpenConnections;
            }
        }


        public ICommand AddConnection
        {
            get
            {
                return new RelayCommand<string>(q =>
                {
                    var z = new ConnectionModel { IsNetConnect = (q == "net") };
                    var vm = new ConnectionViewModel(z);
                    OpenConnections.Add(vm);

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
            EventHandler<ConnectionRemovedEventArgs> handler = ConnectionRemoved;
            if (handler != null) handler(this, e);
        }

        public ICommand RemoveConnection
        {
            get
            {
                return new RelayCommand<ConnectionViewModel>(s =>
                    {
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
    }

    public class ConnectionRemovedEventArgs : EventArgs
    {
        public Common.Connection Connection { get; set; }
    }
}