using Squirrel;
using SquirrelDemoApp.DAL;
using SquirrelDemoApp.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SquirrelDemoApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            new AppUpdater().StartPeriodicAppUpdate(
                @"\\WINSERVER-BUILD\SquirrelDeployments\SquirrelDemoApp",
                TimeSpan.FromSeconds(60),
                ShowUpdateResult, ShowUpdateError);

#if DEBUG
            txtBuildConfiguration.Text = "Build configuration is DEBUG";
#else
            txtBuildConfiguration.Text = "Build configuration is RELEASE";
#endif

            var connectionstring = System.Configuration.ConfigurationManager.ConnectionStrings["TestDB"].ConnectionString;
            var repository = new Repository(connectionstring);
            var dbName = repository.GetDatabaseServerName();
            txtDbServerName.Text = $"Database server is {dbName}";
        }

        private void ShowUpdateResult(UpdateAppResult result)
        {
            //// TEST DATA
            //result = new UpdateAppResult();
            //result.IsInstalledApp = true;
            //result.IsAppUpdated = true;
            //result.UpdateVersion = "4.1.2.3";
            //result.CurrentRunningVersion = "1.2.3";
            //result.Timestamp = DateTime.Now;
            //result.Logs = new List<string>
            //{
            //    "Check for updates succeeded.",
            //    "Download of 5 releases succeeded.",
            //    "Apply releases succeeded. Updated to version: 1.2.4"
            //};

            if (!result.IsInstalledApp)
            {
                AddUpdateMessage("App is not installed. Can't update.", true);
            }
            else
            {
                lblCurrentVersion.Text = result.CurrentRunningVersion;

                foreach (var log in result.Logs)
                    AddUpdateMessage(log, true);

                lbLog.Items.Add("-------------------------------------------------------------------------------------");

                if (result.IsAppUpdated)
                {
                    AddUpdateMessage($"App was updated to new version: {result.UpdateVersion}", false);
                }
            }

            lblLastCheck.Text = result.Timestamp.ToString();
            Background = Brushes.White;
        }

        private void ShowUpdateError(Exception ex)
        {
            var message = $"Exception thrown at: {DateTime.Now.ToString()} \r\n\r\n{ex.ToString()}";
            txtException.Text = message;
            Background = Brushes.Red;
        }

        private void AddUpdateMessage(string message, bool isLog)
        {
            message = $"{DateTime.Now.ToString()}: {message}";

            var lb = isLog ? lbLog : lbUpdates;
            lb.Items.Add(message);
            lb.ScrollIntoView(message);
        }

        private void ButtonRestart_Click(object sender, RoutedEventArgs e)
        {
            UpdateManager.RestartApp();
        }
    }
}
