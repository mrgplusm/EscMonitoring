namespace Monitoring.ViewModel
{

    public class ViewModelLocator
    {
        

        static ViewModelLocator()
        {
            
          //  Communication = new CommunicationViewModel(MainView);
            
            //Email = new SendEmailViewModel();
        }

        private static MainViewModel _mainView;
        public static MainViewModel MainView
        {
            get { return _mainView ?? (_mainView = new MainViewModel()); }            
        }

        //public static CommunicationViewModel Communication { get; set; }


        private static ErrorLogViewModel _errorLog;
        public static ErrorLogViewModel ErrorLog
        {
            get { return _errorLog ?? (_errorLog = new ErrorLogViewModel()); }
        }

        //public static SendEmailViewModel Email { get; set; }

    }
}

