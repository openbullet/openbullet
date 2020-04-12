using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace OpenBullet
{
    /// <summary>
    /// Logica di interazione per App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // Define how to handle unhandled exception
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                OnUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

            Dispatcher.UnhandledException += (s, e) =>
                OnUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException");

            TaskScheduler.UnobservedTaskException += (s, e) =>
                OnUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");
        }

        public void OnUnhandledException(Exception ex, string @event)
        {
            File.AppendAllText(OB.logFile, $"[FATAL][{@event}] UHANDLED EXCEPTION{Environment.NewLine}{ex.ToString()}");
        }
    }
}
