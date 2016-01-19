using Squirrel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SquirrelDemoApp
{
    public sealed class AppUpdater
    {

        public void StartPeriodicAppUpdate(string urlOrPath, 
                                           TimeSpan interval,
                                           Action<UpdateAppResult> handleUpdateResult, 
                                           Action<Exception> handleUpdateError)
        {
            var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            new Func<Task>(() => RunUpdateAndShowResult(urlOrPath, handleUpdateResult, handleUpdateError, taskScheduler))
                .RunPeriodicallyAsync(interval)
                .Forget();
        }

        private Task RunUpdateAndShowResult(string urlOrPath, 
                                            Action<UpdateAppResult> handleUpdateResult,
                                            Action<Exception> handleUpdateError,
                                            TaskScheduler continuationTaskScheduler)
        {
            var updateTask =
                RunAppUpdateAsync(urlOrPath)
                .ContinueWith(async task =>
                {
                    try
                    {
                        UpdateAppResult result = await task.ConfigureAwait(false);
                        handleUpdateResult?.Invoke(result);
                    }
                    catch (Exception ex)
                    {
                        handleUpdateError?.Invoke(ex);
                    }
                }, continuationTaskScheduler);

            return updateTask;
        }


        private async Task<UpdateAppResult> RunAppUpdateAsync(string urlOrPath)
        {
            var result = new UpdateAppResult();
            result.IsInstalledApp = true;

            try
            {
                using (var mgr = new UpdateManager(urlOrPath))
                {
                    if (!mgr.IsInstalledApp)
                    {
                        result.IsInstalledApp = false;
                    }
                    else
                    {
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

                                await mgr.ApplyReleases(updateInfo).ConfigureAwait(false);
                                var newVersion = updateInfo.FutureReleaseEntry.Version.ToString();

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

    }
}
