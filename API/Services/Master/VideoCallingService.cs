using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UneecopsTechnologies.DronaDoctorApp.API.TokenAuth;
using UneecopsTechnologies.DronaDoctorApp.BAL.Interfaces.Master.VideoCalling;
using UneecopsTechnologies.DronaDoctorApp.BAL.Base;
using UneecopsTechnologies.DronaDoctorApp.BAL.Dtos.Master.VideoCalling;

namespace UneecopsTechnologies.DronaDoctorApp.API.Services.Master
{
    public class VideoCallingService
    {
        private readonly IAccessTokenProvider _tokenProvider;
        private readonly IVideoCalling _VideoCalling;
        public VideoCallingService(IAccessTokenProvider tokenProvider, IVideoCalling VideoCalling)
        {
            this._tokenProvider = tokenProvider;
            this._VideoCalling = VideoCalling;
        }

        [FunctionName("FuncForDrAppToGetJwtTokenForVideoCall")]
        public async Task<IActionResult> FuncForDrAppToGetJwtTokenForVideoCall([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetJwtTokenForVideoCall")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetJwtTokenForVideoCall");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }

                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<VideoCallingDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<VideoCallingDto>>(requestBody);

                return new OkObjectResult(_VideoCalling.GetJwtTokenForVideoCall(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
