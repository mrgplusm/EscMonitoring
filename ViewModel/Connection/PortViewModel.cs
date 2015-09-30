using System;

namespace Monitoring.ViewModel.Connection
{
    /*
    public class PortViewModel : IEquatable<PortViewModel>
    {
        public override int GetHashCode()
        {
            return (_port != null ? _port.GetHashCode() : 0);
        }

        private readonly string _port;

        public PortViewModel(string port = null)
        {
            _port = port;
        }

        public bool IsValidPort
        {
            get { return !string.IsNullOrWhiteSpace(_port); }
        }

        public string DisplayName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_port)) return "Select port";
                if (ConnectionViewModel.PortDescriptions.ContainsKey(_port))
                    return _port + " " + ConnectionViewModel.PortDescriptions[_port];
                return _port + "_";
            }
        }


        public string Port
        {
            get { return _port; }

        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PortViewModel);
        }

        public bool Equals(PortViewModel other)
        {
            if (other == null) return false;
            if (ReferenceEquals(other, this)) return true;
            if (other._port == _port) return true;
            return false;
        }
    } */
}