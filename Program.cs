using System;
using System.Windows.Forms;

namespace PixabayWinForms
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Add global exception handling
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            Application.Run(new Form1());
        }

        // Handler for unhandled exceptions in the UI thread
        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            MessageBox.Show($"An error occurred in the application: {e.Exception.Message}\n\nDetails: {e.Exception.StackTrace}",
                "Unexpected Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        // Handler for unhandled exceptions in non-UI threads
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            MessageBox.Show($"A critical error occurred: {ex.Message}\n\nDetails: {ex.StackTrace}",
                "Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
