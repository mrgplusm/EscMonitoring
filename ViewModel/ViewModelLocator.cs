namespace Monitoring.ViewModel
{

    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            MainView = new MainViewModel();

        }


        public MainViewModel MainView { get; }



    }

    static class MainWindow
    {
        static MainWindow()
        {
            MainView = new MainViewModel();
        }

        public static MainViewModel MainView { get; }
    }
}

