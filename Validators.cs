using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Mail;
using System.Text;
using System.Windows.Controls;

namespace Monitoring
{
    public class PortValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            int i;
            if (value != null && int.TryParse(value.ToString(), out i) && i > 0 && i < 65537)
            {
                return new ValidationResult(true, null);
            }
            return new ValidationResult(false, "The value is not a valid e-mail address");
        }
    }

    public class EmailValidationRule : ValidationRule
    {
        private static bool IsValid(string emailaddress)
        {
            try
            {
                new MailAddress(emailaddress);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public override ValidationResult Validate(object value, CultureInfo ultureInfo)
        {
            if (value != null && IsValid(value.ToString()))
            {
                return new ValidationResult(true, null);
            }
            return new ValidationResult(false, "The value is not a valid e-mail address");
        }
    }
}
