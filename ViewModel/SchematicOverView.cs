using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Common;
using Common.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Point = System.Windows.Point;

namespace Monitoring.ViewModel
{
    public class SchematicOverView : ViewModelBase
    {
        private readonly MainViewModel _main;

#if DEBUG

        public SchematicOverView()
        {
            if (IsInDesignModeStatic)
            {
                _main = new MainViewModel();
                Nodes.Add(MainUnitViewModel.GetPanelView(new PanelModel() { Location = new Point(20, 20) }, _main, new MainUnitViewModel()));
                Nodes.Add(MainUnitViewModel.GetPanelView(new PanelModel() { Location = new Point(50, 50) }, _main, new MainUnitViewModel()));

            }

        }
#endif

        public ICommand OpenFile
        {
            get
            {
                return _main.OpenFile;
            }
        }

        public SchematicOverView(MainViewModel main)
        {
            _main = main;

            _main.PasswordEntered += _main_PasswordEntered;            

            if (!LibraryData.SystemIsOpen)
                return;


            if (LibraryData.FuturamaSys.FdsSystems == null)
                LibraryData.FuturamaSys.FdsSystems = new List<FdsModel>();
            AddDevicesToSchematic();

            _main.SchematicBackgroundChanged += (o, e) =>
            {
                _schematicBackground = null;
                RaisePropertyChanged(() => SchematicBackground);
            };


            ActivateMainUnitSchematicErrors();
            MainViewModel.Error += delegate { ActivateMainUnitSchematicErrors(); };
            _main.LogCleared +=(o,e) => ActivateMainUnitSchematicErrors();

            _main.FileChanged += (o,e) => RaisePropertyChanged(() => FileName);
        }

        void _main_PasswordEntered(object sender, EventArgs e)
        {
            RaisePropertyChanged(()=> PasswordEnteredOk);
        }


        public string FileName
        {
            get
            {
                return !LibraryData.SystemIsOpen ? "None" : Path.GetFileNameWithoutExtension(LibraryData.SystemFileName);
            }
        }

        public bool PasswordEnteredOk
        {
            get
            {
                return _main.PasswordEnteredOk;
            }
        }

        public List<DiagramObject> LegendMaterial { get; set; }

        private ImageBrush _schematicBackground;

        public ImageBrush SchematicBackground
        {
            get
            {
                if (!LibraryData.SystemIsOpen) return new ImageBrush();
                if (string.IsNullOrWhiteSpace(LibraryData.FuturamaSys.SchematicBackground)) return new ImageBrush();
                if (!File.Exists(LibraryData.FuturamaSys.SchematicBackground)) return new ImageBrush();
                try
                {
                    //var q = Image.FromFile(LibraryData.FuturamaSys.SchematicBackground);
                    return _schematicBackground ?? (_schematicBackground = new ImageBrush
                    {
                        ImageSource =
                            new BitmapImage(new Uri(LibraryData.FuturamaSys.SchematicBackground, UriKind.Absolute)),
                        //Stretch = Stretch.None,
                        AlignmentX = 0,
                        AlignmentY = 0,
                        //Transform = new ScaleTransform(.5F, .5F)

                    });
                }
                catch (OutOfMemoryException o)
                {
                    Debug.Write("Not an image file " + o);
                }
                return new ImageBrush();
            }
        }

        public ICommand ResetPositions
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var x = 0;
                    var y = 0;
                    foreach (var diagramObject in Nodes)
                    {
                        diagramObject.Location.Value = new Point(x, y);
                        x += 50;
                        if (x % 600 == 0)
                        {
                            x = 0;
                            y += 50;
                        }

                    }
                });
            }
        }

        public double CanvasWidth
        {
            get
            {

                if (SchematicBackground.ImageSource == null || IsInDesignMode)
                    return 600;
                return SchematicBackground.ImageSource.Width * Scaling;
            }
        }

        private void ActivateMainUnitSchematicErrors()
        {
            var latest = MainViewModel.ErrorList.GroupBy(s => s.DeviceError,
                    (x, y) => new { Value = y.OrderByDescending(z => z.Date).First() }).ToArray();

            var active = latest.Where(q => q.Value.Status == ErrorStatuses.FaultSet && q.Value.Status == ErrorStatuses.FaultConfirmed)
           .Select(k => k.Value).Distinct().ToArray();

            foreach (var node in Nodes)
            {
                node.CheckIfError(active);
            }

        }

        public double CanvasHeight
        {
            get
            {

                if (SchematicBackground.ImageSource == null || IsInDesignMode)
                    return 600;
                return SchematicBackground.ImageSource.Height * Scaling;
            }
        }

        public double Scaling
        {
            get
            {
                if (!LibraryData.SystemIsOpen) return 1;
                if (Math.Abs(LibraryData.FuturamaSys.BackgroundScaling) < .1)
                    LibraryData.FuturamaSys.BackgroundScaling = 1;
                return LibraryData.FuturamaSys.BackgroundScaling;
            }
            set
            {
                LibraryData.FuturamaSys.BackgroundScaling = value;
                //SchematicBackground.Transform = new ScaleTransform(Scaling, Scaling);
                RaisePropertyChanged(() => CanvasHeight);
                RaisePropertyChanged(() => CanvasWidth);
                RaisePropertyChanged(() => Scaling);
                RaisePropertyChanged(() => PictScalingValue);
            }
        }

        public double PictScalingValue
        {
            get
            {
                if (!LibraryData.SystemIsOpen) return 1;
                return LibraryData.FuturamaSys.PictogramScaling * Scaling;
            }
        }

        public double PictScaling
        {
            get
            {
                if (!LibraryData.SystemIsOpen) return 1;
                if (Math.Abs(LibraryData.FuturamaSys.PictogramScaling) < .1)
                    LibraryData.FuturamaSys.PictogramScaling = 1;
                return LibraryData.FuturamaSys.PictogramScaling;
            }
            set
            {
                LibraryData.FuturamaSys.PictogramScaling = value;
                RaisePropertyChanged(() => PictScaling);
                RaisePropertyChanged(() => PictScalingValue);
            }
        }

        private int _i;
        private void SetLocation(DiagramObject o)
        {
            if (o.Location.Value.X > 1) return;
            o.Location.Value = new Point(_i, _i);
            _i += 20;
            if (_i > 1000)
                _i = 10;
        }

        private void AddDevicesToSchematic()
        {


            foreach (var q in LibraryData.FuturamaSys.MainUnits.Select(escUnitViewModel => new MainUnitViewModel(escUnitViewModel, _main)))
            {
                if (q.IsVisibileInMonitoringSchematic)
                    AddToSchematic(q);

                foreach (var loudspeakerViewModel in q.SpeakersA.Concat(q.SpeakersB))
                {
                    if (loudspeakerViewModel.IsVisibileInMonitoringSchematic)
                        AddToSchematic(loudspeakerViewModel);
                }

            }

            foreach (var z in _main.MainUnitViewModels)
            {
                foreach (var panelModel in z.Model.AttachedPanelsBus2.Where(n => n.IsVisibileInMonitoringSchematic))
                {
                    AddToSchematic(MainUnitViewModel.GetPanelView(panelModel, _main, z));

                }

            }

            foreach (var fdsSystem in LibraryData.FuturamaSys.FdsSystems)
            {
                AddToSchematic(new FDsViewModel(fdsSystem, _main));
            }
        }

        public FDsViewModel NewFdsModel()
        {

            var n = new FdsModel()
            {
                Id = LibraryData.FuturamaSys.FdsSystems.Count,

            };
            LibraryData.FuturamaSys.FdsSystems.Add(n);

            return new FDsViewModel(n, _main);

        }



        public void AddToSchematic(DiagramObject system, Point location = default(Point))
        {
            if (Nodes.Any(q => q.Equals(system))) return;
            if (!location.Equals(default(Point)))
                system.Location.Value = new Point(location.X / Scaling, location.Y / Scaling);

            SetLocation(system);

            system.RemoveObject += RemoveFromSchematic;
            Nodes.Add(system);
        }

        public void RemoveFromSchematic(object sender, EventArgs eventArgs)
        {
            var system = (DiagramObject) sender;
            if(system == null)return;
            

            var z = Nodes.FirstOrDefault(q => q.Equals(system));

            Nodes.Remove(z);
        }

        private ObservableCollection<DiagramObject> _nodes;
        public ObservableCollection<DiagramObject> Nodes
        {
            get { return _nodes ?? (_nodes = new ObservableCollection<DiagramObject>()); }
        }

        private DiagramObject _selectedObject;



        public DiagramObject SelectedObject
        {
            get
            {
#if DEBUG
                if (IsInDesignMode)
                {
                    return new FDsViewModel(new FdsModel(), new MainViewModel());
                }
#endif
                return _selectedObject;
            }
            set
            {
                foreach (var diagramObject in Nodes)
                {
                    diagramObject.IsHighlighted = false;
                }

                _selectedObject = value;
                RaisePropertyChanged(() => SelectedObject);
            }
        }
    }
}