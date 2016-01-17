using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SquirrelDemoApp
{
    public static class TaskExtensions
    {
        public static Task RunPeriodicallyAsync(this Func<Task> runWork, TimeSpan delay)
        {
            return RunPeriodicallyAsync(runWork, delay, CancellationToken.None);
        }

        public static async Task RunPeriodicallyAsync(this Func<Task> runWork, TimeSpan delay, CancellationToken ct)
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

        public static void Forget(this Task t)
        {
            // Call this on a task when it is not being awaited. (Fire and forget).

            // This will remove the following compiler warning:
            // CS4014: "Because this call is not awaited, execution of the current method continues before the call is completed"
        }
    }
}
