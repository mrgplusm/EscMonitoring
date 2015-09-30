using System;
using System.Globalization;
using System.Net.Mail;
using System.Windows.Controls;

namespace Monitoring.ValidationRules
{
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