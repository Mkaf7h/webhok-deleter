using System;
using System.Windows;

namespace WebhookGUI
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += (s, ev) =>
            {
                MessageBox.Show(ev.ExceptionObject.ToString(), "Critical Error");
            };
            base.OnStartup(e);
        }
    }
}
