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
using UneecopsTechnologies.DronaDoctorApp.BAL.Interfaces.Setting;

namespace UneecopsTechnologies.DronaDoctorApp.API.Services.Security
{
    public class AssistantPermissionServices
    {
        private readonly IAccessTokenProvider _tokenProvider;
        private readonly IUserPermission _UserPermission;
        public AssistantPermissionServices(IAccessTokenProvider tokenProvider, IUserPermission UserPermission)
        {
            this._tokenProvider = tokenProvider;
            this._UserPermission = UserPermission;
        }

        [FunctionName("FuncForDrAppToGetListofPermission")]
        public async Task<IActionResult> FuncForDrAppToGetListofPermission([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetListofPermission")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetListofPermission");
                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<UserPermissions> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<UserPermissions>>(requestBody);

                return new OkObjectResult(_UserPermission.GetListofPermission(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
