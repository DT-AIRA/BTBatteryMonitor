using System;
using System.Threading;
using System.Windows;

namespace BTBatteryMonitor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex? _mutex;
        private const string MutexName = "Global\\BTBatteryMonitor_SingleInstance_Mutex_AppId";

        protected override void OnStartup(StartupEventArgs e)
        {
            _mutex = new Mutex(true, MutexName, out bool isNewInstance);

            // 既に起動中のプロセスが存在する場合は2重起動を抑止
            if (!isNewInstance)
            {
                Shutdown();
                return;
            }

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_mutex != null)
            {
                try
                {
                    _mutex.ReleaseMutex();
                }
                catch { }
                _mutex.Dispose();
                _mutex = null;
            }

            base.OnExit(e);
        }
    }
}
