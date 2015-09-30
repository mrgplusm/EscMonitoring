using System;
using System.Windows;
using GalaSoft.MvvmLight;

namespace Monitoring.ViewModel
{
    public class BindablePoint : ViewModelBase
    {

        public BindablePoint(Point location)
        {
            _value = location;
        }

        public BindablePoint() { }

        public double X
        {
            get { return Value.X; }
            set { Value = new Point(value, Value.Y); }
        }

        public double Y
        {
            get { return Value.Y; }
            set { Value = new Point(Value.X, value); }
        }

        private Point _value;
        public Point Value
        {
            get { return _value; }
            set
            {
                _value = value;
                RaisePropertyChanged(() => Value);
                RaisePropertyChanged(() => X);
                RaisePropertyChanged(() => Y);

                if (ValueChanged != null)
                    ValueChanged();
            }
        }

        public Action ValueChanged;
    }
}
