using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Common;
using Common.Commodules;
using Common.Model;
using GalaSoft.MvvmLight.Command;

namespace Monitoring.ViewModel
{
    public sealed class LoudspeakerViewModel : DiagramObject
    {
        private readonly LoudSpeakerModel _loudSpeaker;
        private readonly MainViewModel _main;
        private readonly MainUnitModel _mainunit;

        public LoudspeakerViewModel(LoudSpeakerModel loudSpeaker, MainViewModel main, MainUnitModel mainunit)
        {
            _loudSpeaker = loudSpeaker;
            _main = main;
            _mainunit = mainunit;


            Location.Value = _loudSpeaker.Location;
            Location.ValueChanged += () => _loudSpeaker.Location = Location.Value;


        }


        private static readonly Dictionary<LoudspeakerLine, Ge> LoudSpeakerLineRelation = new Dictionary<LoudspeakerLine, Ge>
        {
            {LoudspeakerLine.LineA, Ge.SpeakerlineA},
            {LoudspeakerLine.LineB, Ge.SpeakerlineB},            
            {LoudspeakerLine.LineAb, Ge.SpeakerlineA},             //not used
        };

        public override string ContextMenuName
        {
            get { return DisplayName; }
        }

        public override void CheckIfError(IEnumerable<ErrorLineViewModel> activeErrors)
        {
            //take speakerline error instead
            var q = activeErrors.Where(z => z.DeviceError.Number == Id).SelectMany(errorLineViewModel => (errorLineViewModel.DeviceError.GetGraphicalRelations()));
            ErrorActive = q.Any(a => a == LoudSpeakerLineRelation[Line]);
        }

        public override string CustomText
        {
            get { return _loudSpeaker.Name; }
            set
            {
                _loudSpeaker.Name = value;
                RaisePropertyChanged(() => CustomText);
            }
        }

        public override bool IsVisibileInMonitoringSchematic
        {
            get { return _loudSpeaker.IsVisibileInMonitoringSchematic; }
            set
            {
                if (value)
                    _main.AddToMainScreen(this);
                else
                    _main.RemoveFromMainScreen(this);

                _loudSpeaker.IsVisibileInMonitoringSchematic = value;
                RaisePropertyChanged(() => IsVisibileInMonitoringSchematic);
            }
        }

        public override int Id
        {
            get { return _loudSpeaker.Id; }
        }

        public LoudspeakerLine Line
        {
            get { return _loudSpeaker.Line; }
        }

        public override Point Size
        {
            get { return new Point(100, 20); }
        }

        public override string DisplayName
        {
            get
            {
                var sb = new StringBuilder();
                sb.Append("Loudspeaker ");
                sb.Append(ZoneId);
                sb.Append(LspAttr(Line));
                if (!string.IsNullOrWhiteSpace(CustomText))
                {
                    sb.Append(Environment.NewLine);
                    sb.Append(CustomText);
                }                

                return sb.ToString();
            }
        }

        private static string LspAttr(LoudspeakerLine line)
        {
            var field = line.GetType().GetField(line.ToString());

            DescriptionAttribute attribute
                    = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute))
                        as DescriptionAttribute;

            return attribute == null ? line.ToString() : attribute.Description;
        }

        public int ZoneId
        {
            get
            {
                var ret = _loudSpeaker.Id + 1 + 12 * _mainunit.Id;
                if (Line == LoudspeakerLine.LineB) ret -= 12;
                return ret;
            }
        }

        public override Brush Color
        {
            get { return Brushes.Gray; }
        }
    }
}