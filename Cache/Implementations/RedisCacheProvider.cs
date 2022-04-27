using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Net;
using System.Text;
using Cache.Interfaces;

namespace Cache.Implementations
{
    public class RedisCacheProvider : ICacheProvider
    {
        #region Private members
        private static CacheConnectionOptions? connectionOptions;
        private static ConnectionMultiplexer? connection;
        private static IDatabase? cache;
        private readonly ILogger<RedisCacheProvider> logger;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisCacheProvider" /> class.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="options"></param>
        public RedisCacheProvider(ILogger<RedisCacheProvider> logger)
        {
            //Read this from config in actual implementation
            connectionOptions = new CacheConnectionOptions { 
            ConnectionString = "yourcacheconnectionstring",
            ConnectRetry = 3,
            ReConnectRetry = 5,
            TimeToLive =15
            };

            this.logger = logger;
            if (connection == null)
            {
                lock (connectionOptions)
                {
                    if (connection == null)
                    {
                        connection = CreateConnection();
                        cache = connection.GetDatabase();
                    }
                }
            }
        }

        /// <summary>
        /// To check if the connection is alive 
        /// </summary>
        /// <returns></returns>
        public bool IsAlive()
        {
            try
            {
                if (connection == null) return false;
                return connection.IsConnected;
            }
            catch (Exception ex)
            {
                logger.LogError("Redis IsAlive : " + ex.Message, ex.InnerException);
                return false;
            }
        }

        /// <summary>
        /// To flush all keys from all DBs
        /// </summary>
        /// <returns></returns>
        public bool FlushAll()
        {
            bool hasFlushed = false;
            try
            {
                if (connection == null) return false;
                EndPoint[] endpoints = connection.GetEndPoints(true);
                foreach (EndPoint endpoint in endpoints)
                {
                    IServer server = connection.GetServer(endpoint);
                    server.FlushAllDatabases();
                }
                hasFlushed = true;
            }
            catch (Exception)
            {
                hasFlushed = false;
            }
            return hasFlushed;
        }

        /// <summary>
        /// remove cache value with given key Asyncronously, if given key exists else do nothing
        /// </summary>
        /// <param name="key">key name</param>
        public async Task RemoveItemIfExistAsync(string key)
        {
            if (await IsExistAsync(key))
            {
                await RemoveItemAsync(key);
            }
        }

        public async Task<T> Execute<T>(string key, Func<Task<T>> func, int ttl = 0)
        {
            T? data = await GetItemAsync<T>(key);
            if (data == null || data.Equals(default(T)))
            {
                data = await func();
                if (data != null)
                    await SetItemAsync(key, data, ttl);
            }
            return data;
        }
           

        #region Helper methods
        private static T? CreateResponse<T>(byte[] byteData)
        {
            if (byteData == null)
            {
                return default(T);
            }
            string stringData = Encoding.Default.GetString(byteData);
            if (stringData == null)
                return default(T);
            return JsonConvert.DeserializeObject<T>(stringData);
        }
        private static ConnectionMultiplexer CreateConnection()
        {
            if (connectionOptions.TimeToLive == 0)
            {
                connectionOptions.TimeToLive = connectionOptions.TimeToLive;
            }
            if (connectionOptions.ConnectRetry == 0)
            {
                connectionOptions.ConnectRetry = connectionOptions.ConnectRetry;
            }
            if (connectionOptions.ReConnectRetry == 0)
            {
                connectionOptions.ReConnectRetry = connectionOptions.ReConnectRetry;
            }

            ConfigurationOptions options = ConfigurationOptions.Parse(connectionOptions.ConnectionString);
            options.ConnectRetry = connectionOptions.ConnectRetry;
            options.AllowAdmin = true;
            options.ReconnectRetryPolicy = new ExponentialRetry(connectionOptions.ReConnectRetry); 
            return ConnectionMultiplexer.Connect(options);
        }

        /// <summary>
        /// Get cahced value with given key
        /// </summary>
        /// <typeparam name="T">return type</typeparam>
        /// <param name="key">key name</param>
        /// <returns>return value</returns>
        private T? GetItem<T>(string key) where T : class
        {
            try
            {
                if (cache == null) return null;
                byte[] bytes = cache.StringGet(key);
                return CreateResponse<T>(bytes);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get cahced value with given key Asyncronously
        /// </summary>
        /// <typeparam name="T">return type</typeparam>
        /// <param name="key">key name</param>
        /// <returns>return value</returns>
        public async Task<T?> GetItemAsync<T>(string key)
        {
            try
            {
                if (cache == null) return default(T);
                byte[] bytes = await cache.StringGetAsync(key);
                return CreateResponse<T>(bytes);
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// remove cache value with given key
        /// </summary>
        /// <param name="key">key name</param>
        private void RemoveItem(string key)
        {
           if (cache != null) cache.KeyDelete(key);
        }

        /// <summary>
        /// remove cache value with given key Asyncronously
        /// </summary>
        /// <param name="key">key name</param>
        private Task RemoveItemAsync(string key)
        {
            try
            {
                if (cache != null) return cache.KeyDeleteAsync(key);
                return Task.CompletedTask;
            }
            catch
            {
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// remove cache value with given key, if given key exists else do nothing
        /// </summary>
        /// <param name="key">key name</param>
        private void RemoveItemIfExist(string key)
        {
            if (IsExist(key))
            {
                RemoveItem(key);
            }
        }

        /// <summary>
        /// set cache value
        /// </summary>
        /// <param name="key">key name</param>
        /// <param name="value">cache value</param>
        /// <param name="ttl">cache time period in seconds</param>
        private bool SetItem(string key, object value, int ttl = 0)
        {
            if (ttl == 0 && connectionOptions != null)
            {
                ttl = connectionOptions.TimeToLive;
            }

            string data = JsonConvert.SerializeObject(value);
            byte[] byteData = Encoding.Default.GetBytes(data);
            try
            {
                if (cache == null) return false;
                return cache.StringSet(key, byteData, TimeSpan.FromSeconds(ttl));
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// set cache value Asyncronously
        /// </summary>
        /// <param name="key">key name</param>
        /// <param name="value">cache value</param>
        /// <param name="ttl">cache time period in seconds</param>
        private async Task<bool> SetItemAsync(string key, object value, int ttl = 0)
        {
            if (ttl == 0 && connectionOptions != null)
            {
                ttl = connectionOptions.TimeToLive;
            }

            string data = JsonConvert.SerializeObject(value, Formatting.None, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            byte[] byteData = Encoding.Default.GetBytes(data);
            try
            {
                if (cache == null) return false;
                return await cache.StringSetAsync(key, byteData, TimeSpan.FromSeconds(ttl));
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check whether a key exists in cache or not.
        /// </summary>
        /// <param name="key">key name</param>
        /// <returns>True or False</returns>
        private bool IsExist(string key)
        {
            try
            {
                if (cache == null) return false;
                return cache.KeyExists(key);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check whether a key exists in cache or not.
        /// </summary>
        /// <param name="key">key name</param>
        /// <returns>True or False</returns>
        private async Task<bool> IsExistAsync(string key)
        {
            try
            {
                if (cache == null) return false;
                return await cache.KeyExistsAsync(key);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// To check if the connection is alive 
        /// </summary>
        /// <returns></returns>
        private async Task<bool> IsAliveAsync()
        {
            try
            {
                if (connection == null) return false;
                return await Task.FromResult(connection.IsConnected);
            }
            catch (Exception ex)
            {
                logger.LogError("Redis IsAlive Async: " + ex.Message, ex.InnerException);
                return false;
            }
        }
        #endregion
    }
}