using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using Common;
using Common.Commodules;
using Common.Model;
using GalaSoft.MvvmLight.CommandWpf;
using Monitoring.ViewModel.Connection;
using Point = System.Windows.Point;

namespace Monitoring.ViewModel
{
    public sealed class MainUnitViewModel : DiagramObject, ITab
    {
        private readonly MainViewModel _main;

        public MainUnitModel Model { get; }

#if DEBUG
        public MainUnitViewModel()
        {
            Model = LibraryData.EmptyEsc(0);
        }
#endif

        public MainUnitViewModel(MainUnitModel model, MainViewModel main)
        {
            Model = model;
            _main = main;
            UpdateColorOnConnection();

            Location.Value = model.Location;
            ActiveMainUnitErrors = new List<ErrorLineViewModel>();
            Location.ValueChanged += () => model.Location = Location.Value;

            //subscribe event when new error occured.
            MainViewModel.Error += OnError;
            _main.LogCleared += (o,e) =>LogCleared();


            //update errorlist of this mcu
            ActiveMainUnitErrors.AddRange(main.ErrorList.Where(i => i.EscUnit == Id));

        }

        void UpdateColorOnConnection()
        {
            if (_main.CommunicationView == null) return;
            _main.CommunicationView.ConnectionRemoved += DetachHandler;
            foreach (var connection in _main.CommunicationView.Connections)
            {
                AttachHandler(connection.Connection);
            }
        }

        private void AttachHandler(Common.Connection conn)
        {
            conn.ConnectModeChanged += Connection_ConnectModeChanged;
            conn.UnitIdChanged += Connection_ConnectModeChanged;
        }

        private void DetachHandler(object sender, ConnectionRemovedEventArgs connectionRemovedEventArgs)
        {
            if (connectionRemovedEventArgs.Connection == null) return;
            if (connectionRemovedEventArgs.Connection.UnitId == Id)
                Type = ConnectType.None;
            connectionRemovedEventArgs.Connection.ConnectModeChanged -= Connection_ConnectModeChanged;
            connectionRemovedEventArgs.Connection.UnitIdChanged -= Connection_ConnectModeChanged;
        }

        void Connection_ConnectModeChanged(object sender, ConnectionModeChangedEventArgs e)
        {
            if (e.UnitId == Id)
                Type = e.Mode == ConnectMode.Monitoring ? e.Type : ConnectType.None;
        }

        public ConnectType Type
        {
            get { return _type; }
            set
            {
                _type = value;
                RaisePropertyChanged(() => Type);
            }
        }

        private void LogCleared()
        {
            ActiveMainUnitErrors.Clear();
            OnErrorOccured();
        }

        private void OnError(object sender, ErrorLineViewModel errorLineViewModel)
        {
            if (errorLineViewModel.EscUnit != Id || !errorLineViewModel.DataModel.IsVisible) return;
            //add error to this esc's list
            ActiveMainUnitErrors.Add(errorLineViewModel);
            OnErrorOccured();
        }

        /// <summary>
        /// Errors that are currently active for this mainunit
        /// </summary>
        public List<ErrorLineViewModel> ActiveMainUnitErrors { get; set; }

        public event EventHandler ErrorOccured;

        private void OnErrorOccured()
        {
            EventHandler handler = ErrorOccured;
            handler?.Invoke(this, EventArgs.Empty);
        }

        public override string ContextMenuName => UnitName();

        /// <summary>
        /// Checks if there's any error regarding this object
        /// </summary>
        /// <param name="activeErrors"></param>
        public override void CheckIfError(IEnumerable<ErrorLineViewModel> activeErrors)
        {
            var z = activeErrors.Where(q => q.EscUnit == Id).SelectMany(g => g.InvolvedGraphicalUnits());
            ErrorActive = (z.Any(q => q == Ge.EscMaster || q == Ge.EscSlave));
        }

        public override string CustomText
        {
            get { return Model.Name; }
            set
            {
                Model.Name = value;
                RaisePropertyChanged(() => CustomText);
            }
        }

        public override Point Size => new Point(100, 20);

        public override string DisplayName
        {
            get
            {
                var sb = new StringBuilder();
                sb.Append(UnitName());

                if (!string.IsNullOrWhiteSpace(CustomText))
                {
                    sb.Append(Environment.NewLine);
                    sb.Append(CustomText);
                }
                return sb.ToString();
            }
        }


        private string UnitName()
        {
            return Model.Id == 0 ? "Master" : string.Format("slave {0}", (Model.Id));
        }

        public List<bool> ExternalInputs => Model.ExternalInputs ?? (Model.ExternalInputs = new List<bool>(Enumerable.Repeat(true, 9)));
                
        public override int Id => Model.Id;

        public bool FireUsed
        {
            get { return PanelViewModels[0].Any(d => d.IsVisibileInMonitoringSchematic); }
        }

        public bool EvacUsed
        {
            get { return PanelViewModels[1].Any(d => d.IsVisibileInMonitoringSchematic); }
        }

        public bool FdsUsed
        {
            get { return PanelViewModels[2].Any(d => d.IsVisibileInMonitoringSchematic); }
        }

        /// <summary>
        /// Fire, Evac, FDS
        /// </summary>
        public ObservableCollection<PanelViewModel>[] PanelViewModels
        {
            get
            {
                if (_panelViewModels != null) return _panelViewModels;

                var dc = new Dictionary<int, Action>()
                {
                    {0, () => RaisePropertyChanged(() => FireUsed)},
                    {1, () => RaisePropertyChanged(() => EvacUsed)},
                    {2, () => RaisePropertyChanged(() => FdsUsed)},
                };

                _panelViewModels = Enumerable.Range(0, 3).Select(s => new ObservableCollection<PanelViewModel>()).ToArray();

                foreach (var model in Model.AttachedPanelsBus2)
                {
                    PanelViewModels[(int)model.PanelType].Add(GetPanelView(model, _main, this));
                }

                foreach (var action in dc)
                {
                    foreach (var panelViewModel in _panelViewModels[action.Key])
                    {
                        var action1 = action;
                        panelViewModel.VisibilityChanged += (sender, args) => action1.Value();
                    }
                }

                return _panelViewModels;
            }

        }

        public ICommand AddPanelToSchematic
        {
            get
            {
                return new RelayCommand<PanelType>(
                    (s) =>
                    {
                        var q = GetPanelForType(s);
                        if (q == null) return;
                        if (q.IsVisibileInMonitoringSchematic)
                            _main.AddToMainScreen(q);
                    });
            }
        }

        private PanelViewModel GetPanelForType(PanelType type)
        {
            return GetPanelView(Model.AttachedPanelsBus2
                .FirstOrDefault(p => p.PanelType == type), _main, this);
        }


        private ObservableCollection<LoudspeakerViewModel> _speakersA;
        public ObservableCollection<LoudspeakerViewModel> SpeakersA
        {
            get
            {
                return _speakersA ??
                       (_speakersA =
                           new ObservableCollection<LoudspeakerViewModel>(
                               Model.LoudSpeakers.Select(n => new LoudspeakerViewModel(n, _main, Model)).Where(s => s.Line == LoudspeakerLine.LineA)));
            }
        }


        private ObservableCollection<LoudspeakerViewModel> _speakersB;
        private ObservableCollection<PanelViewModel>[] _panelViewModels;
        private ConnectType _type;


        public ObservableCollection<LoudspeakerViewModel> SpeakersB
        {
            get
            {
                return _speakersB ??
                       (_speakersB =
                           new ObservableCollection<LoudspeakerViewModel>(
                               Model.LoudSpeakers.Select(n => new LoudspeakerViewModel(n, _main, Model)).Where(s => s.Line == LoudspeakerLine.LineB)));
            }
        }

        public override Brush Color => Brushes.LightSeaGreen;

        public override bool IsVisibileInMonitoringSchematic
        {
            get
            {
                return Model.IsVisibileInMonitoringSchematic;
            }
            set
            {
                Model.IsVisibileInMonitoringSchematic = value;
                if (value)
                    _main.AddToMainScreen(this);
                else
                    _main.RemoveFromMainScreen(this);

                RaisePropertyChanged(() => IsVisibileInMonitoringSchematic);
            }
        }

        public static PanelViewModel GetPanelView(PanelModel model, MainViewModel main, MainUnitViewModel unit)
        {
            if (model == null) return null;
            switch (model.PanelType)
            {
                case PanelType.Evacuation:
                    return new EvacuationPanelViewModel(model, main, unit);
                case PanelType.Fds:
                    return new FdsPanelViewModel(model, main, unit);
                default: //fire
                    return new FirePanelViewModel(model, main, unit);

            }


        }
        
    }

}