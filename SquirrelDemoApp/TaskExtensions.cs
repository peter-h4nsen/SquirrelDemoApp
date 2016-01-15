using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SquirrelDemoApp
{
    public static class TaskExtensions
    {
        public static void Forget(this Task t)
        {
            // Call this on a task when it is not being awaited. (Fire and forget).

            // This will remove the following compiler warning:
            // CS4014: "Because this call is not awaited, execution of the current method continues before the call is completed"
        }
    }
}
