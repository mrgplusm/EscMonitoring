using System;
using System.Collections.Generic;
using Common;
using Common.Model;
using GalaSoft.MvvmLight;
using Monitoring.UserControls;

namespace Monitoring.ViewModel
{
    public class ErrorLineViewModel : ViewModelBase
    {
        private readonly ErrorLineModel _errorLine;

        public ErrorLineViewModel(ErrorLineModel errorLine)
        {
            _errorLine = errorLine;
        }


        public ErrorLineModel DataModel => _errorLine;

        public IEnumerable<Ge> InvolvedGraphicalUnits()
        {
            return _errorLine.Device.GetGraphicalRelations();
        }

        public ErrorLineViewModel(DeviceError device, ErrorStatuses status, int id, DateTime date, byte[] escdata)
        {

            _errorLine = new ErrorLineModel
                             (
                device,
                                 status,

                                 escdata[0] - 1,

                                 date,
                                 escdata
                             );
            if (escdata.Length != 5)
            {
                throw new ArgumentException("Esc code does not meet requirements (5 bytes)");
            }
        }


        public int EscUnit => _errorLine.EscUnit;

        public string StrEscUnit => EscUnit == 0 ? "Master" : $"Slave {EscUnit}";


        private GroupingError _grouping;
        /// <summary>
        /// Used to group items. Probably this will work correct in most cases
        /// </summary>
        public GroupingError Grouping => _grouping ?? (_grouping = new GroupingError(this));

        public ErrorLineModel ErrorLineModel => _errorLine;

        public int Id => _errorLine.Id;

        public string StrId => (Id + 1).ToString("N0");

        public DateTime Date => _errorLine.Date;

        public string StrDate => _errorLine.Date.ToString("u");

        public DeviceError DeviceError => _errorLine.Device;

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

        public string Description => ErrorNames.ResourceManager.GetString(_errorLine.Device.Detail.ToString()) ?? _errorLine.Device.Detail.ToString();

        public string WhatIf => WhatIfs.ResourceManager.GetString(_errorLine.Device.Detail.ToString()) ?? string.Empty;

        public string StrDetail => UcLog.ResourceManager.GetString(_errorLine.Device.Detail.ToString()) ?? _errorLine.Device.Detail.ToString();

        public ErDt Detail => _errorLine.Device.Detail;

        public string StrStatus => GetStatus(_errorLine.Status);

        public static string GetStatus(ErrorStatuses status)
        {
            return UcLog.ResourceManager.GetString(status.ToString()) ?? status.ToString();
        }

        public ErrorStatuses Status => _errorLine.Status;
    }
}