using System;
using Common.Model;

namespace Monitoring.ViewModel
{
    public class GroupingError : IEquatable<GroupingError>, IComparable, IComparable<GroupingError>
    {
        private readonly ErrorLineViewModel _error;

        public GroupingError(ErrorLineViewModel error)
        {
            _error = error;
        }

        public ErrorLineViewModel Errorline
        {
            get { return _error; }
        }

        public bool Equals(GroupingError other)
        {
            return Errorline.DeviceError.Equals(other.Errorline.DeviceError)
                   && Errorline.EscUnit.Equals(other.Errorline.EscUnit);
        }

        public override int GetHashCode()
        {
            return Errorline.DeviceError.GetHashCode() * 57 ^ 331 * Errorline.EscUnit.GetHashCode();
        }

        public int CompareTo(object obj)
        {
            return CompareTo(obj as GroupingError);
        }

        public int CompareTo(GroupingError other)
        {
            if (other == null) return -1;
            return Errorline.Id - other.Errorline.Id + ((Errorline.Status == ErrorStatuses.FaultSet) ? +1000 : 0);
        }

        public override bool Equals(object obj)
        {
            var other = obj as GroupingError;
            return other != null && Equals(other);
        }
    }
}