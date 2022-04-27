using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using UneecopsTechnologies.DronaDoctorApp.API.TokenAuth;
using UneecopsTechnologies.DronaDoctorApp.BAL.Base;
using UneecopsTechnologies.DronaDoctorApp.BAL.Dtos.Master.HomeAnalytics;
using UneecopsTechnologies.DronaDoctorApp.BAL.Dtos.Setting;
using UneecopsTechnologies.DronaDoctorApp.BAL.Interfaces.Master.HomeAnalytics;


namespace UneecopsTechnologies.DronaDoctorApp.API.Services.Master.HomeAnalytics
{
    public class HomeScreenAnalyticsServices
    {
        private readonly IAccessTokenProvider _tokenProvider;
        private readonly IHomeAnalytics _HomeAnalytics;
        public HomeScreenAnalyticsServices(IAccessTokenProvider tokenProvider, IHomeAnalytics HomeAnalytics)
        {
            this._tokenProvider = tokenProvider;
            this._HomeAnalytics = HomeAnalytics;
        }

        [FunctionName("FuncForDrAppToGetHomeScreenAnalytics")]
        public async Task<IActionResult> FuncForDrAppToGetHomeScreenAnalytics([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetHomeScreenAnalytics")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetHomeScreenAnalytics");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<AnalyticsHomeScreen> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<AnalyticsHomeScreen>>(requestBody);
                return new OkObjectResult(_HomeAnalytics.GetHomeScreenAnalytics(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [FunctionName("FuncForDrAppToGetHomeScreenEarningBreakup")]
        public async Task<IActionResult> FuncForDrAppToGetHomeScreenEarningBreakup([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetHomeScreenEarningBreakup")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetHomeScreenEarningBreakup");
                //var result = _tokenProvider.ValidateToken(req);
                //if (!(result.Status == AccessTokenStatus.Valid))
                //{
                //    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                //    return new UnauthorizedResult();
                //}
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<TotalEarning> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<TotalEarning>>(requestBody);
                return new OkObjectResult(_HomeAnalytics.GetHomeScreenEarningBreakup(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
