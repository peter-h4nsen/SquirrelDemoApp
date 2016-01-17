using Squirrel;
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

            StartPeriodicAppUpdate();
        }

        private void StartPeriodicAppUpdate()
        {
            var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            RunPeriodicallyAsync(
                () => RunUpdateAndShowResult(taskScheduler), 
                TimeSpan.FromSeconds(60)
            ).Forget();
        }

        private Task RunUpdateAndShowResult(TaskScheduler continuationTaskScheduler)
        {
            var updateTask = 
                RunAppUpdateAsync()
                .ContinueWith(async task =>
                {
                    try
                    {
                        UpdateAppResult result = await task.ConfigureAwait(false);
                        ShowUpdateResult(result);
                    }
                    catch (Exception ex)
                    {
                        ShowUpdateError(ex);
                    }
                }, continuationTaskScheduler);

            return updateTask;
        }

        private async Task<UpdateAppResult> RunAppUpdateAsync()
        {
            var result = new UpdateAppResult();
            result.IsInstalledApp = true;

            try
            {
                using (var mgr = new UpdateManager(@"\\WINSERVER-BUILD\SquirrelDeployments\SquirrelDemoApp"))
                {
                    if (!mgr.IsInstalledApp)
                    {
                        result.IsInstalledApp = false;
                    }
                    else
                    {
                        Debugger.Launch();

                        // Contact releases location to get info about updates.
                        UpdateInfo updateInfo = await mgr.CheckForUpdate().ConfigureAwait(false);
                        result.Logs.Add("Check for updates succeeded.");

                        if (updateInfo != null)
                        {
                            List<ReleaseEntry> releases = updateInfo.ReleasesToApply;

                            // If there are pending releases, download them and update the app.
                            if (releases.Any())
                            {
                                await mgr.DownloadReleases(releases).ConfigureAwait(false);
                                result.Logs.Add($"Download of {releases.Count} releases succeeded.");

                                var newVersion = await mgr.ApplyReleases(updateInfo).ConfigureAwait(false);

                                var b = updateInfo.CurrentlyInstalledVersion;

                                result.Logs.Add($"Apply releases succeeded. Updated to version: {newVersion}");
                                result.IsAppUpdated = true;
                                result.UpdateVersion = newVersion;
                            }
                            else
                            {
                                result.Logs.Add("No new releases found.");
                            }
                        }
                        else
                        {
                            result.Logs.Add("UpdateInfo object is null.");
                        }

                        // Always gets the version currently running - also if the app has been updated while running!
                        result.CurrentRunningVersion = mgr.CurrentlyInstalledVersion().ToString();
                    }
                }
            }
            catch (Exception ex) when (ex.Message.StartsWith("Update.exe not found"))
            {
                result.IsInstalledApp = false;
            }

            result.Timestamp = DateTime.Now;
            return result;
        }

        private void ShowUpdateResult(UpdateAppResult result)
        {
            // TEST DATA
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

        private Task RunPeriodicallyAsync(Func<Task> runWork, TimeSpan delay)
        {
            return RunPeriodicallyAsync(runWork, delay, CancellationToken.None);
        }

        private async Task RunPeriodicallyAsync(Func<Task> runWork, TimeSpan delay, CancellationToken ct)
        {
            if (runWork == null)
                return;

            while (true)
            {
                ct.ThrowIfCancellationRequested();

                await runWork().ConfigureAwait(false);
                await Task.Delay(delay, ct).ConfigureAwait(false);
            }
        }

        
    }
}
