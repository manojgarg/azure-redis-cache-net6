
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
using UneecopsTechnologies.DronaDoctorApp.BAL.Dtos.Master.AboutUs;
using UneecopsTechnologies.DronaDoctorApp.BAL.Interfaces.Master.AboutUs;

namespace UneecopsTechnologies.DronaDoctorApp.API.Services.Master.AboutUs
{
    public class AboutUsService
    {
        private readonly IAccessTokenProvider _tokenProvider;
        private readonly IAboutUs _AboutUs;

        public AboutUsService(IAccessTokenProvider tokenProvider, IAboutUs AboutUs)
        {
            this._tokenProvider = tokenProvider;
            this._AboutUs = AboutUs;
        }

        [FunctionName("FuncForDrAppToGetAboutUsDetails")]
        public async Task<IActionResult> FuncForDrAppToGetAboutUsDetails([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetAboutUsDetails")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetAboutUsDetails");
                //var result = _tokenProvider.ValidateToken(req);
                //if (!(result.Status == AccessTokenStatus.Valid))
                //{
                //    return new OkObjectResult(new WrapperStandardOutput<string>("-1", "Unauthorized"));
                //    return new UnauthorizedResult();
                //}
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<AboutUsInputDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<AboutUsInputDto>>(requestBody);

                return new OkObjectResult(_AboutUs.GetAboutUsDetails(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForAdminToGetVersionHistoryDetails")]
        public async Task<IActionResult> FuncForAdminToGetVersionHistoryDetails([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForAdminToGetVersionHistoryDetails")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForAdminToGetVersionHistoryDetails");
                //var result = _tokenProvider.ValidateToken(req);
                //if (!(result.Status == AccessTokenStatus.Valid))
                //{
                //    return new OkObjectResult(new WrapperStandardOutput<string>("-1", "Unauthorized"));

                //}
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                WrapperStandardInput<VersionHistoryInput> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<VersionHistoryInput>>(requestBody);
                return new OkObjectResult(_AboutUs.AdminGetVersionHistoryDetails(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToAddNewVersionHistoryDetails")]
        public async Task<IActionResult> FuncForDrAppToAddNewVersionHistoryDetails([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAddNewVersionHistoryDetails")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAddNewVersionHistoryDetails");


                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                WrapperStandardInput<VersionHistoryInput> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<VersionHistoryInput>>(requestBody);

                return new OkObjectResult(_AboutUs.AddNewVersionHistoryDetails(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }


    }
}
