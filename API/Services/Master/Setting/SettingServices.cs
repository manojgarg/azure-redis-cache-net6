using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UneecopsTechnologies.DronaDoctorApp.API.TokenAuth;
using UneecopsTechnologies.DronaDoctorApp.BAL.Base;
using UneecopsTechnologies.DronaDoctorApp.BAL.Common;
using UneecopsTechnologies.DronaDoctorApp.BAL.Dtos.Setting;
using UneecopsTechnologies.DronaDoctorApp.BAL.Interfaces.Setting;

namespace UneecopsTechnologies.DronaDoctorApp.API.Services.Master.Setting
{
    public class SettingServices
    {
        private readonly IAccessTokenProvider _tokenProvider;
        private readonly ISetting _Setting;
        public SettingServices(IAccessTokenProvider tokenProvider, ISetting Setting)
        {
            this._tokenProvider = tokenProvider;
            this._Setting = Setting;
        }

        [FunctionName("FuncForDrAppToGetListofSettings")]
        public async Task<IActionResult> FuncForDrAppToGetListofSettings([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetListofSettings")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetListofSettings");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<UserSettings> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<UserSettings>>(requestBody);
                return new OkObjectResult(_Setting.GetListofSettings(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [FunctionName("FuncForDrAppToSaveUpdateSettings")]
        public async Task<IActionResult> FuncForDrAppToSaveUpdateSettings([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToSaveUpdateSettings")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToSaveUpdateSettings");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<UserSettings> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<UserSettings>>(requestBody);
                return new OkObjectResult(_Setting.SaveUpdateSettings(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToSaveCardDisplayStatus")]
        public async Task<IActionResult> FuncForDrAppToSaveCardDisplayStatus([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToSaveCardDisplayStatus")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToSaveCardDisplayStatus");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<CommonFilterSortDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<CommonFilterSortDto>>(requestBody);
                return new OkObjectResult(_Setting.SaveCardDisplayStatus(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
