using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebounceMyMouse.UI.Models
{
    public class MouseEventLogEntry
    {
        public DateTime Timestamp { get; set; }
        public string InputType { get; set; }
        public bool IsBlocked { get; set; }
        public TimeSpan? Delta { get; set; }

        public string LogMessage
        {
            get
            {
                string results = $"{Timestamp:HH:mm:ss.fff} - {InputType}";
                if (IsBlocked && Delta.HasValue)
                    results += $" (Δ {Delta.Value.TotalMilliseconds:0} ms)";
                return results;
            }
        }

        public string BackgroundColor => IsBlocked ? "#FFFFCCCC" : "White";
    }
}
