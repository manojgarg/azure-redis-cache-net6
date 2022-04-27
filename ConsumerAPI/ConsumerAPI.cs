using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using BAL.Interfaces;

namespace ConsumerAPI
{
    public class ConsumerAPI
    {

        IDataService _dataService;

        public ConsumerAPI(IDataService dataService)
        {
            _dataService = dataService;
        }

        [FunctionName("ConsumerAPI")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string key = req.Query["key"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            key = key ?? data?.key;

            string responseMessage = string.IsNullOrEmpty(key)
                ? "Key is not valid."
                : await _dataService.GetData(key);

            return new OkObjectResult(responseMessage);
        }
    }
}
