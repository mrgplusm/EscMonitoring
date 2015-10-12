using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Common;
using Common.Commodules;
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

            if (LibraryData.SystemIsOpen)
            {
                if (LibraryData.FuturamaSys.Connections == null) return;
                foreach (var c in LibraryData.FuturamaSys.Connections.Select(model => new ConnectionViewModel(model)))
                {
                    OpenConnections.Add(c);
                  //  c.Connection.CreateConnection(c.Ipaddress, c.ConnectType);
                }
            }       
        }

        public void OnPasswordEnteredOk()
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

        public ICommand RemoveConnection
        {
            get
            {
                return new RelayCommand<ConnectionViewModel>(s =>
                    {
                        //stop connection
                        s.EndConnection();

                        //remove from list

                        //remove from view
                        OpenConnections.Remove(s);

                        if (!LibraryData.SystemIsOpen) return;
                        if (LibraryData.FuturamaSys.Connections == null)
                        {
                            LibraryData.FuturamaSys.Connections = new List<ConnectionModel>();
                            return;
                        }
                        LibraryData.FuturamaSys.Connections.Remove(s.DataModel);
                    });
            }
        }

        public void ReconnectAllDisconnected()
        {
            foreach (var c in Connections
                .Where(connectionViewModel => connectionViewModel.ConnectMode != ConnectMode.Monitoring))
            {
               // c.Connection.CreateConnection(c.Ipaddress, c.ConnectType);
            }
        }
    }
}