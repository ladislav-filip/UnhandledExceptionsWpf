using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace UnhandledExceptionsWpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private SynchronizationContext _synchronizationContext;
        private int _thrId;

        public App()
        {
            _thrId = Thread.CurrentThread.ManagedThreadId;
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            _synchronizationContext = SynchronizationContext.Current;

            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;            

            var mainWind = new MainWindow();
            var model = new MainWindowViewModel();
            model.Title = "Moje aplikace";
            mainWind.DataContext = model;

            mainWind.Show();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("CurrentDomain_UnhandledException");
            var excep = (Exception)e.ExceptionObject;
            if (Thread.CurrentThread.ManagedThreadId != _thrId)
            {
                _synchronizationContext.Post(o =>
                {
                    var ex = (Exception)o;
                    ShowError(ex, "UnhandledException POST");
                }, excep.GetBaseException());
            }
            else
            {
                ShowError(excep.GetBaseException(), "UnhandledException");
                Environment.Exit(-1);
            }
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("TaskScheduler_UnobservedTaskException");
            if (Thread.CurrentThread.ManagedThreadId != _thrId)
            {
                _synchronizationContext.Post(o =>
                {
                    var ex = (Exception) o;
                    ShowError(ex, "UnobservedTaskException POST");
                }, e.Exception.GetBaseException());
            }
            else
            {
                ShowError(e.Exception.GetBaseException(), "UnobservedTaskException");
            }
        }

        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("App_OnDispatcherUnhandledException");            
            e.Handled = e.Exception is ApplicationException;
            if (e.Handled)
            {
                ShowError(e.Exception, "OnDispatcherUnhandledException");
            }
        }

        private void ShowError(Exception e, string title)
        {
            MessageBox.Show("An unhandled exception just occurred: " + e.Message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
