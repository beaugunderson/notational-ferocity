using System.Windows;

using NLog;

namespace NotationalFerocity
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected override void OnStartup(StartupEventArgs e)
        {
            Logger.Info("Application starting.");

            Current.DispatcherUnhandledException += OnDispatcherUnhandledException;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Logger.Info("Application exiting.");

            base.OnExit(e);
        }

        private bool showingFatal;

        void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            if (showingFatal)
            {
                e.Handled = true;

                return;
            }

            showingFatal = true;

            Logger.Error("Fatal exception ".PadLeft(80, '*'));

            try
            {
                int level = 0;

                var ex = e.Exception;

                while (ex != null)
                {
                    Logger.Error(string.Format("Fatal exception level {0}: ", level++), ex);
                    
                    ex = ex.InnerException;
                }
            }
            finally
            {
               Logger.Error("Fatal exception end ".PadLeft(80, '*'));
            }

            Shutdown();
        }
    }
}