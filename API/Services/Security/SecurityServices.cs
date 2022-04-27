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
using UneecopsTechnologies.DronaDoctorApp.BAL.Dtos.Security;
using UneecopsTechnologies.DronaDoctorApp.BAL.Interfaces;

namespace UneecopsTechnologies.DronaDoctorApp.API.Services.Security
{
    public class SecurityServices
    {
        private readonly IAccessTokenProvider _tokenProvider;
        private readonly ISecurity _Security;
        public SecurityServices(IAccessTokenProvider tokenProvider, ISecurity Security)
        {
            this._tokenProvider = tokenProvider;
            this._Security = Security;
        }

        [FunctionName("FuncForDocAppToPushLoginData")]
        public async Task<IActionResult> FuncForWebAppToPushPocData([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForWebAppToPushPocData")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDocAppToPushLoginData");
                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<LoginModel> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<LoginModel>>(requestBody);

                return new OkObjectResult(_Security.ValidateAdminLoginInfo(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
