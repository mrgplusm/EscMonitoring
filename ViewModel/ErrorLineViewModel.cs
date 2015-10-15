using System;
using System.Collections.Generic;
using Common;
using Common.Model;

using GalaSoft.MvvmLight;
using Monitoring.UserControls;

namespace Monitoring.ViewModel
{
    public class ErrorLineViewModel : ViewModelBase, IEquatable<ErrorLineViewModel>, IComparable, IComparable<ErrorLineViewModel>
    {
        private readonly ErrorLineModel _errorLine;

        public ErrorLineViewModel(ErrorLineModel errorLine)
        {
            _errorLine = errorLine;
        }


        public ErrorLineModel DataModel
        {
            get { return _errorLine; }
        }

        public IEnumerable<Ge> InvolvedGraphicalUnits()
        {
            return _errorLine.Device.GetGraphicalRelations();
        }

        public ErrorLineViewModel(DeviceError device, ErrorStatuses status, int id, DateTime date, byte[] escdata)
        {

            _errorLine = new ErrorLineModel
                             {
                                 Device = device,
                                 Status = status,
                                 EscUnit = escdata[0] - 1,
                                 Id = id,
                                 Date = date
                             };
            if (escdata.Length != 5)
            {
                throw new ArgumentException("Esc code does not meet requirements (5 bytes)");
            }



            _errorLine.EscData = escdata;
        }


        public int EscUnit
        {
            get { return _errorLine.EscUnit; }
        }

        public string StrEscUnit
        {
            get
            {
                return EscUnit == 0 ? "Master" : string.Format("Slave {0}", EscUnit);
            }
        }



        private GroupingError _grouping;
        /// <summary>
        /// Used to group items. Probably this will work correct in most cases
        /// </summary>
        public GroupingError Grouping
        {
            get { return _grouping ?? (_grouping = new GroupingError(this)); }
        }

        public ErrorLineModel ErrorLineModel { get { return _errorLine; } }

        public int Id
        {
            get { return _errorLine.Id; }
        }

        public string StrId
        {
            get { return (Id + 1).ToString("N0"); }
        }

        public DateTime Date
        {
            get
            {
                return _errorLine.Date;
            }
        }

        public string StrDate
        {
            get { return _errorLine.Date.ToString("u"); }
        }

        public DeviceError DeviceError
        {
            get { return _errorLine.Device; }
        }

        public string StrDevice
        {
            get
            {
                var dvn = DeviceError.Module == SyMo.External || DeviceError.Module == SyMo.Internal
                    ? string.Empty
                    : (DeviceError.Number + 1).ToString("N0");
                var trstr = UcLog.ResourceManager.GetString(DeviceError.Module.ToString());
                if (trstr == null) return DeviceError.Module + " " + dvn;
                return string.Format(trstr, dvn);
            }
        }

        public string Description
        {
            get
            {
                return ErrorNames.ResourceManager.GetString(_errorLine.Device.Detail.ToString()) ?? _errorLine.Device.Detail.ToString();
            }
        }

        public string WhatIf
        {
            get
            {
                return WhatIfs.ResourceManager.GetString(_errorLine.Device.Detail.ToString()) ?? string.Empty;
            }
        }

        public string StrDetail
        {
            get
            {
                return UcLog.ResourceManager.GetString(_errorLine.Device.Detail.ToString()) ?? _errorLine.Device.Detail.ToString();
            }
        }

        public ErDt Detail
        {
            get { return _errorLine.Device.Detail; }
        }

        public string StrStatus
        {
            get { return GetStatus(_errorLine.Status); }
        }

        public static string GetStatus(ErrorStatuses status)
        {
            return ErrorNames.ResourceManager.GetString(status.ToString()) ?? status.ToString();
        }

        public ErrorStatuses Status
        {
            get { return _errorLine.Status; }
        }

        public virtual bool Equals(ErrorLineViewModel other)
        {
            return ReferenceEquals(this, other) ||
             _errorLine.Equals(other._errorLine);
        }

        public override int GetHashCode()
        {
            return _errorLine.GetHashCode();
        }

        public int CompareTo(object obj)
        {
            return CompareTo(obj as ErrorLineViewModel);
        }

        public int CompareTo(ErrorLineViewModel other)
        {
            if (other == null) return -1;
            return other.Id - Id;
        }

        public override bool Equals(object obj)
        {
            var other = obj as ErrorLineViewModel;
            if (other == null) return false;
            return Equals(other);
        }
    }
}