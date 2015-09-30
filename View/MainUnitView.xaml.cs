using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Common;
using Common.Model;
using Microsoft.Research.DynamicDataDisplay.Charts.Shapes;
using Monitoring.UserControls;
using Monitoring.ViewModel;

namespace Monitoring.View
{



    /// <summary>
    /// Interaction logic for MainUnitView.xaml
    /// </summary>
    public partial class MainUnitView : UserControl
    {
        private MainUnitViewModel _dataContext;

        public MainUnitView()
        {
            InitializeComponent();
            


            _borderStoryboard = FindResource("ErrorStopStoryboard") as Storyboard;
            _arrowStoryboard = FindResource("ArrowStoryboard") as Storyboard;
           

            foreach (var errorType in Enum.GetValues(typeof(Ge)).Cast<Ge>())
            {
                AddStoryBoardToControl(errorType);
            }

            
        }

        void MainUnitView_Loaded(object sender, RoutedEventArgs e)
        {
            SetHandlers();
        }

        public void SetHandlers()
        {

            _dataContext = (MainUnitViewModel)DataContext;
            _dataContext.ErrorOccured += DisplayError;
            DisplayError();   
        }


        private RoutedEvent e;

        private readonly Storyboard _borderStoryboard;
        private readonly Storyboard _arrowStoryboard;
      

        private void DisplayError()
        {
            //errors with latest date

            var latest = _dataContext.ActiveMainUnitErrors.GroupBy(s => s.DeviceError,
                (x, y) => new { Value = y.OrderByDescending(z => z.Date).First() }).ToArray();

            var active = latest.Where(q => q.Value.Status == ErrorStatuses.FaultSet)
                .SelectMany(k => k.Value.InvolvedGraphicalUnits())
                .Distinct().ToArray();


            //list shown and not in latest
            foreach (var removeThis in _errorsShown.Except(active).ToArray())
            {
                _errorsShown.Remove(removeThis);
                _storyboards[removeThis].Stop();
            }

            //list in list and not shown
            foreach (var addThis in active.Except(_errorsShown).ToArray())
            {
                _errorsShown.Add(addThis);
                _storyboards[addThis].Begin();
            }

        }

        private void AddStoryBoardToControl(Ge controlName)
        {

            switch (controlName)
            {
                case Ge.Fire://4
                case Ge.Fds://6
                case Ge.Evacuation://5
                case Ge.Amplifier://0
                case Ge.BackupAmplifier://1
                case Ge.PowerSource230Vac://22
                case Ge.EscMaster://12
                case Ge.EnteroEpc://13
                case Ge.EscSlave:                    
                    var control = FindName(controlName.ToString()) as DependencyObject;
                    if (control == null)
                        throw new Exception("cannot find control " + controlName + " in mainunit");
                    var qq = _borderStoryboard.Clone();
                    Storyboard.SetTarget(qq, control);
                    _storyboards.Add(controlName, qq);
                    break;
                case Ge.EnteroEsa230Vac:
                case Ge.RedundancyModule:
                case Ge.EnteroEsaAmpPsu:
                case Ge.Circuitbreaker:
                case Ge.EnteroEpc230Vac:
                case Ge.EnteroEpcContact:
                case Ge.ExtAudioIn:
                    var usercontrol = FindName(controlName.ToString()) as ExternalInput;
                    if (usercontrol == null) throw new Exception("cannot find control " + controlName + " in mainunit");
                    var qq1 = _borderStoryboard.Clone();
                    usercontrol.ErrorStoryboard = qq1;
                    _storyboards.Add(controlName, qq1);
                    break;
                case Ge.ExtAudioInConnection:
                case Ge.ExtErrorInConnection:
                case Ge.MainsErrorContactConnection:
                case Ge.EpcConnection:
                case Ge.SpeakerlineA:
                case Ge.SpeakerlineB:
                case Ge.PanelbusFp:
                case Ge.PanelbusEp:
                case Ge.PanelbusFds:
                case Ge.EscLink:
                case Ge.PowerSource230VacConnectie:
                case Ge.Esc48VdcConnection:
                    var control2 = FindName(controlName.ToString()) as DependencyObject;
                    if (control2 == null)
                        throw new Exception("cannot find control " + controlName + " in mainunit");
                    var qq2 = _arrowStoryboard.Clone();
                    Storyboard.SetTarget(qq2, control2);
                    _storyboards.Add(controlName, qq2);
                    break;
                case Ge.ExpansionFirePanel:
                case Ge.ExpansionEvacuation:
                case Ge.ExpansionFds:
                    break;
                default:
                    throw new Exception("All graphical errors should exist in this switch statement");
            }

        }

        private readonly HashSet<Ge> _errorsShown = new HashSet<Ge>();
        private readonly Dictionary<Ge, Storyboard> _storyboards = new Dictionary<Ge, Storyboard>();

    }

    public static class ThemeProperties
    {
        public static Brush GetTickBrush(DependencyObject obj)
        {
            return (Brush)obj.GetValue(TickBrushProperty);
        }

        public static void SetTickBrush(DependencyObject obj, Brush value)
        {
            obj.SetValue(TickBrushProperty, value);
        }

        public static readonly DependencyProperty TickBrushProperty =
            DependencyProperty.RegisterAttached(
                "TickBrush",
                typeof(Brush),
                typeof(ThemeProperties),
                new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.Inherits));


    }
}
