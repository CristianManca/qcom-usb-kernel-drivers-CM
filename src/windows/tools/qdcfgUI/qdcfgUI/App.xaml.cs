using QCUtility;
using QDCFGUtility;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;

namespace qdcfgUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        void ProcessArguments(object sender, StartupEventArgs e)
        {
            string lib0 = "qdcfg.dll";
            string lib1 = "qcdev.dll";
            string dir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

            if (!File.Exists(Path.Combine(dir, lib0)))
            {
                MessageBox.Show(string.Format("{0} does not exist", lib0));
                Environment.Exit(1);
            }

            if (!File.Exists(Path.Combine(dir, lib1)))
            {
                MessageBox.Show(string.Format("{0} does not exist", lib1));
                Environment.Exit(1);
            }

            if (e.Args.Length > 0)
            {
                // start as a command line tool
                int retCode = QDCFG.processCommand(e.Args.Length, e.Args);
                Environment.Exit(retCode);
            }
            else
            {
                // hide console window
                AppOnStartup(sender, e);
                ConsoleController.ShowWindow(ConsoleController.GetConsoleWindow(), 0);
            }
        }

        private const string UniqueEventName = "qdcfgUIStartUpEvent";
        private const string UniqueMutexName = "qdcfgUIStartUpMutex";
        private EventWaitHandle eventWaitHandle;
        private Mutex mutex;

        private void AppOnStartup(object sender, StartupEventArgs e)
        {
            bool isOwned;
            mutex = new Mutex(true, UniqueMutexName, out isOwned);
            eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, UniqueEventName);

            if (isOwned)
            {
                // Spawn a thread which will be waiting for our event
                var thread = new Thread(
                    () =>
                    {
                        while (eventWaitHandle.WaitOne())
                        {
                            Current.Dispatcher.BeginInvoke((Action)(() => ((MainWindow)Current.MainWindow).BringToForeground()));
                        }
                    });

                thread.IsBackground = true;
                thread.Start();
            }
            else
            {
                eventWaitHandle.Set();
                Shutdown();
            }
        }
    }
}
