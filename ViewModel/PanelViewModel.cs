using System;
using System.Text;
using Common.Model;

namespace Monitoring.ViewModel
{
    public abstract class PanelViewModel : DiagramObject
    {
        protected readonly PanelModel Panel;
        protected readonly MainViewModel Main;
        protected readonly MainUnitViewModel Unit;

        protected PanelViewModel(PanelModel panel, MainViewModel main, MainUnitViewModel unit)
        {
            Panel = panel;
            Main = main;
            Unit = unit;
            Location.Value = panel.Location;
            Location.ValueChanged += () => panel.Location = Location.Value;
        }

        public override int Id
        {
            get { return Panel.Id; }
        }

        public override bool IsVisibileInMonitoringSchematic
        {
            get
            {
                return Panel.IsVisibileInMonitoringSchematic;
            }
            set
            {
                if (value)
                    Main.AddToMainScreen(this);
                else
                    Main.RemoveFromMainScreen(this);

                Panel.IsVisibileInMonitoringSchematic = value;
                RaisePropertyChanged(() => IsVisibileInMonitoringSchematic);
                OnVisibilityChanged();
            }
        }

        public event EventHandler VisibilityChanged;

        public sealed override string DisplayName
        {
            get
            {
                var sb = new StringBuilder();
                sb.Append(string.Format(UnitName, Panel.Id + 1));
                
                if (!string.IsNullOrWhiteSpace(CustomText))
                {
                    sb.Append(Environment.NewLine);
                    sb.Append(CustomText);
                }

                sb.Append(Environment.NewLine);
                sb.Append(Unit.DisplayName);
                return sb.ToString();
            }
        }

        public sealed override string ContextMenuName
        {
            get
            {
                return string.Format(UnitName, Panel.Id + 1 ); 
            }
        }

        public abstract string UnitName { get; }

        protected virtual void OnVisibilityChanged()
        {
            var handler = VisibilityChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}