using ForzaRadioModTool.Core.Services;
using ForzaRadioModTool.UI.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Windows.Forms;

namespace ForzaRadioModTool
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // Configure global error handling
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += OnThreadException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            try
            {
                // Configure application
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // Setup dependency injection
                var services = new ServiceCollection();
                services.AddForzaRadioServices();

                var serviceProvider = services.BuildServiceProvider();

                // Create and run main form
                using var scope = serviceProvider.CreateScope();
                var mainForm = new MainForm(scope.ServiceProvider);
                
                Application.Run(mainForm);
            }
            catch (Exception ex)
            {
                HandleStartupError(ex);
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static void OnThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            Log.Error(e.Exception, "Unhandled thread exception occurred");
            HandleError(e.Exception, "Application Error");
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                Log.Fatal(ex, "Unhandled domain exception occurred. Terminating: {IsTerminating}", e.IsTerminating);
                HandleError(ex, "Critical Application Error");
            }
        }

        private static void HandleStartupError(Exception ex)
        {
            var message = $"Failed to start application:\n\n{ex.Message}";
            if (ex.InnerException != null)
            {
                message += $"\n\nInner Exception:\n{ex.InnerException.Message}";
            }

            MessageBox.Show(message, "Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private static void HandleError(Exception ex, string title)
        {
            var message = $"An error occurred:\n\n{ex.Message}";
            
            // Don't show full stack trace to user, but log it
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}