using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using Common.Commodules;
using Common.Converters;
using Common.Model;
using Monitoring.ViewModel;

namespace Monitoring.Converters
{
    [ValueConversion(typeof(ReadOnlyObservableCollection<ValidationError>), typeof(string))]
    public class ValidationErrorsToStringConverter : MarkupExtension, IValueConverter
    {
        public ValidationErrorsToStringConverter() { }

        public object Convert(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            var errors =
                value as ReadOnlyObservableCollection<ValidationError>;

            if (errors == null)
            {
                return string.Empty;
            }

            return string.Join("\n", (from e in errors
                                      select e.ErrorContent as string).ToArray());
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new ValidationErrorsToStringConverter();
        }
    }


    public class AnythingCheckedConverter : BaseConverter, IValueConverter
    {
        #region IValueConverter Members

        public AnythingCheckedConverter() { }

        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            return ((ObservableCollection<PanelViewModel>)value).All(s=> !s.IsVisibileInMonitoringSchematic);
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
    
    public class ErrorlineHeaderConverter : BaseConverter, IValueConverter
    {
        #region IValueConverter Members

        public ErrorlineHeaderConverter() { }

        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            var t = value as GroupingError;
            if (t == null) return string.Empty;
            return t.Errorline.StrEscUnit + " " + t.Errorline.StrDevice + " " + t.Errorline.StrDetail;

        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            throw  new NotImplementedException("Errorlineheaderconverter");
        }

        #endregion
    }
}