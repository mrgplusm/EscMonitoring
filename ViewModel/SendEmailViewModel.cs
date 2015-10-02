using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Windows.Input;
using Common;
using Common.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

using Monitoring.Email;


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


    public class SendEmailViewModel : ViewModelBase
    {



        public SendEmailViewModel()
        {

#if DEBUG
            if (IsInDesignMode)
                LibraryData.CreateEmptySystem();
#endif
            if (!LibraryData.SystemIsOpen) return;

            if (LibraryData.FuturamaSys.Email == null)
                LibraryData.FuturamaSys.Email = GetEmailModel();

            
            ProjectDetails = new ObservableCollection<DetailField>(Enum.GetValues(typeof(OrganisationDetail))
                .OfType<OrganisationDetail>().Select((n) => new DetailField(n, OrganisationType.Project)));

            DealerDetails = new ObservableCollection<DetailField>(Enum.GetValues(typeof(OrganisationDetail))
                .OfType<OrganisationDetail>().Select((n) => new DetailField(n, OrganisationType.Dealer)));

            Addresses = new ObservableCollection<StringWrapper>(LibraryData.FuturamaSys.Email.Receivers.Select((n, i) => new StringWrapper(i, n)));
            Addresses.CollectionChanged += ReceiversOnCollectionChanged;
        }

        public static SendEmailModel GetEmailModel()
        {
            return new SendEmailModel()
            {
                OrganisationDetails = Enumerable.Range(0, 2)
                    .Select(q => Enumerable.Range(0, 8).Select(s => string.Empty).ToArray())
                    .ToArray(),
                Receivers = new List<string>()
            };
        }


        public ObservableCollection<DetailField> ProjectDetails { get; private set; }
        public ObservableCollection<DetailField> DealerDetails { get; private set; }
        public ObservableCollection<StringWrapper> Addresses { get; private set; }

        /// <summary>
        ///     Sinchronize witch backing property
        /// </summary>

        private static void ReceiversOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems.OfType<StringWrapper>())
                {
                    LibraryData.FuturamaSys.Email.Receivers.Remove(item.Value);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems.OfType<StringWrapper>())
                {
                    LibraryData.FuturamaSys.Email.Receivers.Add(item.Value);
                }
            }
        }



        #region Sender

        public string SenderFrom
        {
            get { return LibraryData.FuturamaSys.Email.SenderFrom; }
            set { LibraryData.FuturamaSys.Email.SenderFrom = value; }
        }

        public string SenderDisplay
        {
            get { return LibraryData.FuturamaSys.Email.SenderDisplay; }
            set { LibraryData.FuturamaSys.Email.SenderDisplay = value; }
        }

        public string SenderSmtpServer
        {
            get { return LibraryData.FuturamaSys.Email.SenderSmtpServer; }
            set { LibraryData.FuturamaSys.Email.SenderSmtpServer = value; }
        }

        public string SenderSmtpPort
        {
            get { return LibraryData.FuturamaSys.Email.SenderSmtpPort.ToString(CultureInfo.InvariantCulture); }
            set
            {
                int t;
                int.TryParse(value, out t);
                LibraryData.FuturamaSys.Email.SenderSmtpPort = t;
            }
        }


        public ICommand AddEmail
        {
            get
            {
                return new RelayCommand(() => Addresses.Add(new StringWrapper(Addresses.Count, "entero_va_support@bose.com")),
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
                    LibraryData.FuturamaSys.Email.IsSenderSsl;
            }
            set
            {
                LibraryData.FuturamaSys.Email.IsSenderSsl = value;
            }
        }


        public string SenderUsername
        {
            get { return LibraryData.FuturamaSys.Email.SenderUsername; }
            set { LibraryData.FuturamaSys.Email.SenderUsername = value; }
        }

        public string SenderPassword
        {
            get { return LibraryData.FuturamaSys.Email.SenderPassword; }
            set { LibraryData.FuturamaSys.Email.SenderPassword = value; }
        }


        #endregion

        public bool InspectorCleared
        {
            get
            {
                return LibraryData.FuturamaSys.Email.InspectorCleared;    
            }
            set
            {
                LibraryData.FuturamaSys.Email.InspectorCleared = value;    
                RaisePropertyChanged(() => InspectorCleared);
            }   
        }

        public static void SendEmail()
        {
            if (!LibraryData.SystemIsOpen) return;
            if (LibraryData.TestMode) return;
            var model = LibraryData.FuturamaSys.Email;

            var smtpClient = new SmtpClient();
            //var wpfEmailer = new WPFEmailer();

            var mailMessage = new MailMessage();

            if (model == null || model.SenderFrom == null || model.SenderDisplay == null ||
                LibraryData.FuturamaSys.Errors == null || model.SenderSmtpPort == 0 || model.Receivers.Count < 1)
            {
                Debug.WriteLine("Not all neccesarily fields were filed in, email not send ");
                return;
            }

            foreach (var address in model.Receivers)
            {
                mailMessage.To.Add(new MailAddress(address));
            }

            

            mailMessage.From = new MailAddress(model.SenderFrom, model.SenderDisplay);
            mailMessage.Subject = "New Entero VA error";


            mailMessage.IsBodyHtml = true;

            //.OrderByDescending(q=> q.LogCleared).Take(10)

            var body = new FormattedMail
            {
                Model = model,
                ErrorsToSend = LibraryData.FuturamaSys.Errors.OrderByDescending(q => q.Date).Take(10).ToList(),
                ClearedErrors =
                    LibraryData.FuturamaSys.ClearedErrors.OrderByDescending(q => q.LogCleared).Take(10).ToList(),
                Connections = LibraryData.FuturamaSys.Connections
            }; 

            mailMessage.Body = body.TransformText();
            mailMessage.Attachments.Add(new Attachment(LibraryData.SystemFileName));


           // File.WriteAllText("mail.html", mailMessage.Body);

            smtpClient.Host = model.SenderSmtpServer;
            smtpClient.Port = model.SenderSmtpPort;
            smtpClient.EnableSsl = model.IsSenderSsl;            
            smtpClient.Credentials = new NetworkCredential(model.SenderUsername, model.SenderPassword);

            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.SendCompleted += SendCompletedCallback;

            try
            {
                smtpClient.SendAsync(mailMessage, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private static void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            // Get the unique identifier for this asynchronous operation.
            // var token = (ErrorLineModel)e.UserState;
            /*
            if (e.Cancelled)
            {
                Debug.WriteLine("[{0}] Send canceled.", token);
            }
            if (e.Error != null)
            {
                Debug.WriteLine("[{0}] {1}", token.Date, e.Error);
            }
            else
            {
                Debug.WriteLine("Message sent.");
            }
             */
            // token.EmailSend = true;
        }
    }
}