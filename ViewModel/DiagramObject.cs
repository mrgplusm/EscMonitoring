using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

using System.Windows.Input;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace Monitoring.ViewModel
{
    public abstract class DiagramObject : ViewModelBase
    {
        public abstract string DisplayName { get; }       

        private bool _errorActive;
        /// <summary>
        /// Activates the error indicator in overview
        /// </summary>
        public bool ErrorActive
        {
            get { return _errorActive; }
            protected set
            {
                if (_errorActive == value) return;
                _errorActive = value;
                RaisePropertyChanged(() => ErrorActive);
            }
        }

        public abstract string ContextMenuName { get; }

        public abstract void CheckIfError(IEnumerable<ErrorLineViewModel> activeErrors);

        public abstract string CustomText { get; set; }

        public abstract Point Size { get; }

        public abstract Brush Color { get; }

        public abstract bool IsVisibileInMonitoringSchematic { get; set; }

        public event EventHandler RemoveObject;

        protected virtual void OnRemoveObject()
        {
            EventHandler handler = RemoveObject;
            if (handler != null) handler(this, EventArgs.Empty);
        }


        public ICommand RemoveThis
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (RemoveObject != null)
                    {
                        IsVisibileInMonitoringSchematic = false;
                        OnRemoveObject();

                    }

                });
            }
        }

        private BindablePoint _location;
        public virtual BindablePoint Location
        {
            get
            {
                return _location ?? (_location = new BindablePoint());
            }
        }

        private bool _isHighlighted;
        public bool IsHighlighted
        {
            get
            {
                return _isHighlighted;
            }
            set
            {
                _isHighlighted = value;
                RaisePropertyChanged(() => IsHighlighted);
            }
        }

        public abstract int Id { get; }        
    }
}