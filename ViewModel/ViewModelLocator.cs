namespace Monitoring.ViewModel
{

    public class ViewModelLocator
    {       
        static ViewModelLocator()
        {
            _mainView = new MainViewModel();
            _errorLog = new ErrorLogViewModel();
        }

        public ViewModelLocator()
        {

        }

        private static MainViewModel _mainView;
        public static MainViewModel MainView
        {
            get { return _mainView; }// _mainView; }            
        }

        //public static CommunicationViewModel Communication { get; set; }

        static ErrorLogViewModel _errorLog;

        public static ErrorLogViewModel ErrorLog
        {
            get { return _errorLog; }
        }

        //public static SendEmailViewModel Email { get; set; }

    }
}

