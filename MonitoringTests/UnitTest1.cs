using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Common;
using Common.Model;
using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monitoring;
using Monitoring.Email;
using Monitoring.ViewModel;
using Point = System.Windows.Point;

namespace MonitoringTests
{
    [TestClass]
    public class UnitTest1
    {
        private SendEmailModel Testmodel
        {
            get
            {
                var q = new SendEmailViewModel();

                var d = q.Email;

                d.InspectorCleared = true;
                d.SenderSmtpServer = "smtp.bose.com";
                d.Receivers = new List<string>(new[] { "boris_bergman@bose.com" });
                d.SenderFrom = "boris_bergman@bose.com";
                d.SenderDisplay = "Boris Bergman";
                d.SenderSmtpPort = 25;
                return d;


            }
        }


        [TestMethod]
        public void SendEmailTest()
        {
            LibraryData.OpenSystem(LibraryData.EmptySystemModel());
            var t = new EmailSender(Testmodel);
            t.SendEmail();
        }


        [TestMethod]
        public void PasswordTest()
        {
            const string password = "TestPW*&^/";
            var mainView = new MainViewModel();
            mainView.SetPassword(password);

            mainView.EnteredPassword(password + ";");

            Assert.IsFalse(mainView.PasswordEnteredOk);

            mainView.EnteredPassword(password);
            Assert.IsTrue(mainView.PasswordEnteredOk);

            mainView.SetPassword(string.Empty);
            Assert.IsTrue(mainView.PasswordEnteredOk);

            mainView.EnteredPassword("2673");
            Assert.IsTrue(mainView.PasswordEnteredOk);

        }

        [TestMethod]
        public void TestEmailMessage()
        {
            LibraryData.OpenSystem(new FuturamaSysModel());

            var view = new FormattedMail
            {
                Model = Testmodel
            };

            view.Model.InspectorCleared = true;

            var mailTest = view.TransformText();

            Assert.IsTrue(mailTest.Contains("Log was cleared during an inspection of the VA system"));


            var view2 = new FormattedMail
            {
                Model = Testmodel
            };

            view2.Model.InspectorCleared = false;

            var mailTest2 = view2.TransformText();

            Assert.IsFalse(mailTest2.Contains("inspection"));
        }

        [TestMethod]
        public void InterPretBackupAmp()
        {
            var code = new byte[] { 0x01, 0x81, 0x0A, 0x1F, 0x01, 0x1B, 0xBC, 0x00, 0x01, 0x84 };
           
            ErrorLineModel errorLine;

            ErrorCodes.GetErrorFromEscCode(code, out errorLine);

            Assert.AreEqual(errorLine.EscUnit, 0);
            Assert.AreEqual(errorLine.Device.Detail, ErDt.AmpDefect);
            Assert.AreEqual(errorLine.Device.Number, 0);
            Assert.AreEqual(errorLine.Device.Module, SyMo.BackupAmplifier);
            Assert.AreEqual(errorLine.Status, ErrorStatuses.FaultReseted);

        }

        [TestMethod]
        public void InterpretSpeakerLine()
        {
            //# speakerline 5a open line set 27
            var code = new byte[] { 0x01, 0x81, 0x0A, 0x1F, 0x01, 0x09, 0x05, 0x00, 0x04, 0xBE };
            
            ErrorLineModel errorLine;

            ErrorCodes.GetErrorFromEscCode(code, out errorLine);

            Assert.AreEqual(errorLine.EscUnit, 0);
            Assert.AreEqual(errorLine.Device.Detail, ErDt.OpenlineA);
            Assert.AreEqual(errorLine.Device.Number, 4);
            Assert.AreEqual(errorLine.Device.Module, SyMo.Speakerline);            
            Assert.AreEqual(errorLine.Status, ErrorStatuses.FaultSet);

        }

        [TestMethod]
        public void InterpretMainsError()
        {
            //#mains error contact open reset 48
            var code = new byte[] { 0x02, 0x81, 0x0A, 0x1F, 0x02, 0x17, 0xA1, 0x00, 0x20, 0x86 };
            
            ErrorLineModel errorLine;

            ErrorCodes.GetErrorFromEscCode(code, out errorLine);

            Assert.AreEqual( 1, errorLine.EscUnit);
            Assert.AreEqual(ErDt.ImpDev1KhzA, errorLine.Device.Detail);
            //Assert.AreEqual(47, errorLine.Device.Number);
            Assert.AreEqual(SyMo.Internal, errorLine.Device.Module);
            Assert.AreEqual(ErrorStatuses.FaultReseted, errorLine.Status);

        }

        [TestMethod]
        public void InterpretFpMicCapError()
        {
            //fp1 mic cap open ack
            var code = new byte[] { 0x01, 0x81, 0x0A, 0x1F, 0x01, 0x07, 0x21, 0x00, 0x01, 0xD5 };
            
            ErrorLineModel errorLine;

            ErrorCodes.GetErrorFromEscCode(code, out errorLine);

            Assert.AreEqual(0, errorLine.EscUnit);
            Assert.AreEqual(SyMo.Fire, errorLine.Device.Module);
            Assert.AreEqual(ErDt.MicCapOpenLine, errorLine.Device.Detail);
            Assert.AreEqual(0, errorLine.Device.Number);
            
            Assert.AreEqual(ErrorStatuses.FaultConfirmed, errorLine.Status);

        }


    }

}
