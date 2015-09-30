using Common;

namespace Monitoring.ViewModel
{
    public class DetailField
    {
        private readonly OrganisationDetail _detail;
        private readonly OrganisationType _type;

        public DetailField(OrganisationDetail detail, OrganisationType type)
        {
            _detail = detail;
            _type = type;
        }

        public string DetailName
        {
            get { return _detail.ToString(); }
        }

        public string DetailValue
        {
            get { return GetProperty(_detail, _type); }
            set
            {
                SetProperty(_detail, _type, value);
            }
        }


        private string GetProperty(OrganisationDetail detail, OrganisationType type)
        {
            return LibraryData.FuturamaSys.Email.OrganisationDetails[(int)type][(int)detail];
        }

        private void SetProperty(OrganisationDetail detail, OrganisationType type, string value)
        {
            LibraryData.FuturamaSys.Email.OrganisationDetails[(int)type][(int)detail] = value;
        }
    }
}