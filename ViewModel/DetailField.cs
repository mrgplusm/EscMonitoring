using Common;
using Common.Model;

namespace Monitoring.ViewModel
{
    public class DetailField
    {
        private readonly OrganisationDetail _detail;
        private readonly OrganisationType _type;
        private readonly SendEmailModel _model;

        public DetailField(SendEmailModel model, OrganisationDetail detail, OrganisationType type)
        {
            _detail = detail;
            _type = type;
            _model = model;
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
            return _model.OrganisationDetails[(int)type][(int)detail];
        }

        private void SetProperty(OrganisationDetail detail, OrganisationType type, string value)
        {
            _model.OrganisationDetails[(int)type][(int)detail] = value;
        }
    }
}