
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Common;
using Common.Model;

using Point = System.Windows.Point;

namespace Monitoring.ViewModel
{
    public class EvacuationPanelViewModel : PanelViewModel
    {
        private readonly PanelModel _panel;

        public EvacuationPanelViewModel(PanelModel panel, MainViewModel main, MainUnitViewModel unit)
            : base(panel, main, unit)
        {
            _panel = panel;
        }

        public override void CheckIfError(IEnumerable<ErrorLineViewModel> activeErrors)
        {            
            //take speakerline error instead
            var q = activeErrors.Where(z => z.DeviceError.Number == Id).SelectMany(errorLineViewModel => (errorLineViewModel.DeviceError.GetGraphicalRelations()));
            ErrorActive = (q.Any(a => a == Ge.Evacuation));

        }

        public override string CustomText
        {
            get { return _panel.Name; }
            set
            {
                _panel.Name = value;
                RaisePropertyChanged(() => CustomText);
            }
        }

        public override Point Size
        {
            get { return new Point(40, 30); }
        }

 
        public override Brush Color
        {
            get { return Brushes.Green; }
        }

        public override string UnitName
        {
            get { return "Evacuation Panel {0}"; }
        }
    }
}