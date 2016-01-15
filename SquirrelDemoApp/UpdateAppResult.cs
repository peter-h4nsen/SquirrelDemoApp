using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SquirrelDemoApp
{
    public class UpdateAppResult
    {
        public UpdateAppResult()
        {
            Logs = new List<string>();
        }

        public bool IsInstalledApp { get; set; }
        public bool IsAppUpdated { get; set; }
        public string UpdateVersion { get; set; }
        public string CurrentRunningVersion { get; set; }
        public DateTime Timestamp { get; set; }
        public List<string> Logs { get; set; }
    }
}
