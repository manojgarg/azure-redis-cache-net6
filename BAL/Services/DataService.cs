using BAL.Interfaces;
using Cache.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAL.Services
{
    public class DataService: IDataService
    {

        ICacheProvider _cache;

        public DataService()
        { }
        public DataService(ICacheProvider cache)
        {
            _cache = cache;
        }

        public async Task<String> GetData(string Key)
        {
            return await _cache.Execute(Key, () =>
            {
                return Task.FromResult(GetDataFromDB(Key));
            }, 15);
        }

        private String GetDataFromDB(string key)
        {
            //This is just for demonstration on a seperate 
            //method for fetching data via DB  not actual
            //implementation
            return  $"randomA{key}";
        }



    }
}
