using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cache.Interfaces;

namespace Cache
{
    public class RedisHealthCheck : IHealthCheck
    {
        private ICacheProvider _cacheProvider;
        public RedisHealthCheck(ICacheProvider cacheProvider)
        {
            _cacheProvider = cacheProvider;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            return _cacheProvider.IsAlive()
                ? Task.FromResult(HealthCheckResult.Healthy("Redis cache Healthy"))
                : Task.FromResult(HealthCheckResult.Degraded("Redis cache Degraded"));
        }
    }
}
