using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using Common;
using Common.Model;

namespace Monitoring.ViewModel
{
    public sealed class FDsViewModel : DiagramObject
    {
        private readonly FdsModel _fds;
        private readonly MainViewModel _main;

        public FDsViewModel(FdsModel fds, MainViewModel main)
        {
            _fds = fds;
            _main = main;

            Location.Value = fds.Location;
            Location.ValueChanged += () => fds.Location = Location.Value;

            RemoveObject += WhenRemoved;
        }

        private void WhenRemoved(DiagramObject diagramObject)
        {

            var q = diagramObject as FDsViewModel;
            if (q == null) return;
            if (!LibraryData.SystemIsOpen || LibraryData.FuturamaSys.FdsSystems == null) return;
            LibraryData.FuturamaSys.FdsSystems.Remove(q.DataModel);

        }

        public override string DisplayName
        {
            get
            {
                var sb = new StringBuilder();
                sb.Append("Fire detection system ");
                
                if (!string.IsNullOrWhiteSpace(CustomText))
                {
                    sb.Append(Environment.NewLine);
                    sb.Append(CustomText);
                }

                return sb.ToString();
            }
        }

        public override string ContextMenuName
        {
            get { return DisplayName; }
        }

        public override void CheckIfError(IEnumerable<ErrorLineViewModel> activeErrors)
        {

        }

        public FdsModel DataModel
        {
            get { return _fds; }
        }

        public override string CustomText
        {
            get { return _fds.Name; }
            set
            {
                _fds.Name = value;
                RaisePropertyChanged(() => CustomText);
            }
        }


        public override Point Size
        {
            get { return new Point(100, 20); }
        }

        public override Brush Color
        {
            get { return Brushes.SeaGreen; }
        }

        public override bool IsVisibileInMonitoringSchematic { get; set; }

        public override int Id
        {
            get { return _fds.Id; }
        }
    }
}