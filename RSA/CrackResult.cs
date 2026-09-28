using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSA
{
    internal class CrackResult
    {
        public string MethodName { get; set; }
        public bool IsSuccess { get; set; }
        public long FoundValue { get; set; }
        public long FoundP { get; set; }
        public long FoundQ { get; set; }
        public long Attempts { get; set; }
        public TimeSpan ElapsedTime { get; set; }
        public TimeSpan EstimatedTotalTime { get; set; }
        public bool TimeoutReached { get; set; }
        public string Comment { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
