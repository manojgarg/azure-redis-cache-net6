using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cache
{
    public class CacheConnectionOptions
    {
        public string? ConnectionString { get; set; }
        public int TimeToLive { get; set; }
        public int ConnectRetry { get; set; }
        public int ReConnectRetry { get; set; }
    }
}
