using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebounceMyMouse.Core
{
    public class Channel
    {
        public string Name { get; set; }
        public TimeSpan DebounceTime { get; set; }
        private Stopwatch _timer = Stopwatch.StartNew();
        public Channel(string name, TimeSpan debounceTime)
        {
            Name = name;
            DebounceTime = debounceTime;
        }

        public bool ShouldBlock()
        {
            bool results;
            if (_timer.Elapsed <= DebounceTime)
            {
                results = true;
            }
            else
            {
                _timer.Restart();
                results = false;

            }
            
            return results;
        }
    }
}
