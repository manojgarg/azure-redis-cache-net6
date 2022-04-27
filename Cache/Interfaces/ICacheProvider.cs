using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cache.Interfaces
{
    public interface ICacheProvider
    {
        /// <summary>
        /// Check if the cache connection is alive - required for healthCheck
        /// </summary>
        /// <returns></returns>
        bool IsAlive();

        /// <summary>
        /// To flush all keys from all DBs
        /// </summary>
        /// <returns></returns>
        bool FlushAll();

        /// <summary>
        /// remove cache value with given key Asyncronously, if given key exists else do nothing
        /// </summary>
        /// <param name="key">key name</param>
        Task RemoveItemIfExistAsync(string key);

        /// <summary>
        /// To get and set data in cache directly.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="func"></param>
        /// <param name="ttl"></param>
        /// <returns></returns>
        Task<T> Execute<T>(string key, Func<Task<T>> func, int ttl = 0);

        Task<T?> GetItemAsync<T>(string key);


    }
}
