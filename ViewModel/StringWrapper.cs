using Common;
using GalaSoft.MvvmLight;

namespace Monitoring.ViewModel
{
    public class StringWrapper : ViewModelBase
    {
        private int _position;
        private string _value;

        public StringWrapper(int position, string value)
        {
            _position = position;
            _value = value;
        }

        public int index
        {
            get { return _position; }
            set { _position = value; }
        }

        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                LibraryData.FuturamaSys.Email.Receivers[_position] = value;
                RaisePropertyChanged(() => Value);
            }
        }
    }
}