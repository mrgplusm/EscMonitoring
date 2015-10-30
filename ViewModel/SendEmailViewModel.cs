using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Common;
using Common.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;


namespace Monitoring.ViewModel
{
    public enum OrganisationDetail
    {
        Name = 0,
        Address = 1,
        ZipCode = 2,
        Place = 3,
        Country = 4,
        Phone = 5,
        ContactPerson = 6,
        EmailAddress = 7,
    }

    public enum OrganisationType
    {
        Project = 0,
        Dealer = 1,
    }


    public class SendEmailViewModel : ViewModelBase, ITab
    {
        public SendEmailViewModel()
        {

#if DEBUG
            if (IsInDesignMode)
                LibraryData.CreateEmptySystem();
#endif
            if (!LibraryData.SystemIsOpen) return;

            ProjectDetails = new ObservableCollection<DetailField>(Enum.GetValues(typeof(OrganisationDetail))
                .OfType<OrganisationDetail>().Select((n) => new DetailField(Email, n, OrganisationType.Project)));

            DealerDetails = new ObservableCollection<DetailField>(Enum.GetValues(typeof(OrganisationDetail))
                .OfType<OrganisationDetail>().Select((n) => new DetailField(Email, n, OrganisationType.Dealer)));

            Addresses =
                new ObservableCollection<StringWrapper>(
                    LibraryData.FuturamaSys.Email.Receivers.Select((n, i) => new StringWrapper(i, n)));
            Addresses.CollectionChanged += ReceiversOnCollectionChanged;
        }

        public SendEmailModel Email
        {
            get
            {
                if (LibraryData.FuturamaSys.Email != null) return LibraryData.FuturamaSys.Email;

                LibraryData.FuturamaSys.Email = new SendEmailModel()
                {
                    OrganisationDetails = Enumerable.Range(0, 2)
                        .Select(q => Enumerable.Range(0, 8).Select(s => string.Empty).ToArray())
                        .ToArray(),
                    Receivers = new List<string>()
                };

                return LibraryData.FuturamaSys.Email;
            }
        }

        public ObservableCollection<DetailField> ProjectDetails
        {
            get; private set;
        }

        public ObservableCollection<DetailField> DealerDetails { get; private set; }
        public ObservableCollection<StringWrapper> Addresses { get; private set; }

        /// <summary>
        ///     Sinchronize witch backing property
        /// </summary>

        private void ReceiversOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems.OfType<StringWrapper>())
                {
                    Email.Receivers.Remove(item.Value);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems.OfType<StringWrapper>())
                {
                    Email.Receivers.Add(item.Value);
                }
            }
        }

        #region Sender

        public string SenderFrom
        {
            get { return Email.SenderFrom; }
            set { Email.SenderFrom = value; }
        }

        public string SenderDisplay
        {
            get { return Email.SenderDisplay; }
            set { Email.SenderDisplay = value; }
        }

        public string SenderSmtpServer
        {
            get { return Email.SenderSmtpServer; }
            set { Email.SenderSmtpServer = value; }
        }

        public string SenderSmtpPort
        {
            get
            {
                return Email.SenderSmtpPort.ToString(CultureInfo.InvariantCulture);
            }
            set
            {
                int t;
                int.TryParse(value, out t);
                Email.SenderSmtpPort = t;
            }
        }


        public ICommand AddEmail
        {
            get
            {
                return
                    new RelayCommand(
                        () => Addresses.Add(new StringWrapper(Addresses.Count, "entero_va_support@bose.com")),
                        () => Addresses != null && Addresses.Count < 8);
            }
        }


        public ICommand RemoveEmail
        {
            get
            {
                return new RelayCommand<StringWrapper>((q) =>
                {
                    Addresses.Remove(q);
                    var qq = Addresses.Select((a, i) => a.index = i).ToArray();
                }, (q) => Addresses.Count > 0);
            }
        }

        public bool SenderSsl
        {
            get
            {
                return
                    Email.IsSenderSsl;
            }
            set { Email.IsSenderSsl = value; }
        }


        public string SenderUsername
        {
            get { return Email.SenderUsername; }
            set { Email.SenderUsername = value; }
        }

        public string SenderPassword
        {
            get { return Email.SenderPassword; }
            set { Email.SenderPassword = value; }
        }


        #endregion

        public bool InspectorCleared
        {
            get { return Email.InspectorCleared; }
            set
            {
                Email.InspectorCleared = value;
                RaisePropertyChanged(() => InspectorCleared);
            }
        }

        public int Id => 100;
    }
}