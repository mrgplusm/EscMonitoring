using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Timers;
using Common;
using Common.Model;
using Monitoring.Email;

namespace Monitoring.ViewModel
{
    public class EmailSender
    {
        private readonly SendEmailModel _model;
        
        

        
        

        static EmailSender()
        {
         
        }

        public EmailSender(SendEmailModel model1)
        {
            _model = model1;
            
        }

        

        

        private bool EmailCanBeSend()
        {

            if (!LibraryData.SystemIsOpen) return false;            

            if (_model?.SenderFrom != null && 
                _model.SenderDisplay != null && 
                LibraryData.FuturamaSys.Errors != null && 
                _model.SenderSmtpPort != 0 && 
                _model.Receivers.Count >= 1 && 
        
                _model.SendEmailEnabled)
                return true;
            Debug.WriteLine("Not all neccesarily fields were filed in or timer is not resetted, email not send ");
            return false;
        }

        private string CreateMailBody()
        {
            var body = new FormattedMail
            {
                Model = _model,
                ErrorsToSend = LibraryData.FuturamaSys.Errors.OrderByDescending(q => q.Date).Take(10).ToList(),

                Connections = LibraryData.FuturamaSys.Connections
            };

            if (LibraryData.FuturamaSys.ClearedErrors != null)
                body.ClearedErrors = LibraryData.FuturamaSys.ClearedErrors.OrderByDescending(q => q.LogCleared).Take(10).ToList();

            try
            {
                return body.TransformText();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error formatting email" + e);
                return string.Empty;
            }
        }


        private MailMessage MessageFactory()
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_model.SenderFrom, _model.SenderDisplay),
                Subject = "New Entero VA error",
                IsBodyHtml = true,
                Body = CreateMailBody(),

            };
            if (File.Exists(LibraryData.SystemFileName))
                mailMessage.Attachments.Add(new Attachment(LibraryData.SystemFileName));

            foreach (var address in _model.Receivers)
            {
                mailMessage.To.Add(new MailAddress(address));
            }

            return mailMessage;
        }

        private SmtpClient ClientFactory()
        {
            // ReSharper disable once UseObjectOrCollectionInitializer
            var smtpClient = new SmtpClient
            {
                UseDefaultCredentials = false,
                Host = _model.SenderSmtpServer,
                Port = _model.SenderSmtpPort,
                EnableSsl = _model.IsSenderSsl,                
                Timeout = 2000,
            };

            //this has to be done seperately due to .NET bug
            //https://medium.com/developer-developers-developers/send-emails-through-office365-exchange-online-using-net-69b3a4a3b236
            smtpClient.Credentials = new NetworkCredential(_model.SenderUsername, _model.SenderPassword);

            smtpClient.SendCompleted += SendCompletedCallback;

            return smtpClient;
        }

        private readonly object _sendLock = new object();

        private Task SendMail(SmtpClient client, MailMessage message)
        {
            return Task.Factory.StartNew(() =>
            {
                lock (_sendLock)
                {
                    client.Send(message);
                }
            });

        }

        public async void SendEmail()
        {
            if (!EmailCanBeSend()) return;

            var mailMessage = MessageFactory();
            var smtpClient = ClientFactory();

            try
            {
                await SendMail(smtpClient, mailMessage);             
                
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

            if (e.Cancelled)
            {
                Debug.WriteLine("[{0}] Send canceled.");
            }
            if (e.Error != null)
            {

            }
            else
            {
                Debug.WriteLine("Message sent.");
            }
            
            // token.EmailSend = true;
        }
    }
}