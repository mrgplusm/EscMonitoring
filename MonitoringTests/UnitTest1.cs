using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
        [TestMethod]
        public void TestMethod1()
        {
            var main = new MainViewModel();
            var mainUnit = new MainUnitModel();
            var vm = new MainUnitViewModel(mainUnit, main);

            var ch = new SchematicOverView(main);

            var z = ch.PictScaling;
            var x = ch.Scaling;
            var y = ch.PictScalingValue;

            Assert.AreEqual(1, x);
            Assert.AreEqual(1, y);
            Assert.AreEqual(1, z);
        }

        [TestMethod]
        public void Temp()
        {

            var q = DateTime.Now.ToString("u") + '\t' + "sdfsdf";

            var z = q.Split('\t');
            Console.WriteLine(z[0] + "-" + z[1]);
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
            var view = new FormattedMail
            {
                Model = SendEmailViewModel.GetEmailModel(),
            };

            view.Model.InspectorCleared = true;

            var mailTest = view.TransformText();

            Assert.IsTrue(mailTest.Contains("The log has been cleared by inspector"));


            var view2 = new FormattedMail
            {
                Model = SendEmailViewModel.GetEmailModel(),
            };

            view2.Model.InspectorCleared = false;
            
            var mailTest2 = view2.TransformText();

            Assert.IsFalse(mailTest2.Contains("inspector"));

        }

        [TestMethod]
        public void TestEmailMessageErrorList()
        {
            var view = new FormattedMail
            {
                Model = SendEmailViewModel.GetEmailModel(),
                ErrorsToSend =
                    new List<ErrorLineModel>()
                    {
                        new ErrorLineModel()
                        {
                            Device = new DeviceError() {Detail = ErDt.AmpDefect, Module = SyMo.BackupAmplifier},
                            EmailSend = false,
                            Status = ErrorStatuses.FaultSet
                        }
                    },
            };

            var text =view.TransformText();

            Assert.IsTrue(text.Contains("AmpDefect"));
            Assert.IsTrue(text.Contains("Backup Amplifier"));
            
        }
        
    }

}
