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
using UneecopsTechnologies.DronaDoctorApp.BAL.Dtos.Master.DoctorProfile;
using UneecopsTechnologies.DronaDoctorApp.BAL.Interfaces.Master.DoctorProfile;

namespace UneecopsTechnologies.DronaDoctorApp.API.Services.Master.DoctorProfile
{
    public class DoctorProfileService
    {
        private readonly IAccessTokenProvider _tokenProvider;
        private readonly IDoctorProfile _DoctorProfile;

        public DoctorProfileService(IAccessTokenProvider tokenProvider, IDoctorProfile DoctorProfile)
        {
            this._tokenProvider = tokenProvider;
            this._DoctorProfile = DoctorProfile;
        }

        [FunctionName("FuncForDrAppToGetDoctorProfile")]
        public async Task<IActionResult> FuncForDrAppToGetDoctorProfile([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetDoctorProfile")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetDoctorProfile");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<DoctorProfileDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<DoctorProfileDto>>(requestBody);

                return new OkObjectResult(_DoctorProfile.GetDoctorProfile(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToUpdateAboutDoctor")]
        public async Task<IActionResult> FuncForDrAppToUpdateAboutDoctor([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToUpdateAboutDoctor")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToUpdateAboutDoctor");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<DoctorProfileDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<DoctorProfileDto>>(requestBody);

                return new OkObjectResult(_DoctorProfile.UpdateAboutDoctor(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToAddUpdateEducation")]
        public async Task<IActionResult> FuncForDrAppToAddUpdateEducation([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAddUpdateEducation")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAddUpdateEducation");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<EducationDetails> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<EducationDetails>>(requestBody);

                return new OkObjectResult(_DoctorProfile.AddUpdateEducation(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToAddUpdateExperience")]
        public async Task<IActionResult> FuncForDrAppToAddUpdateExperience([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAddUpdateExperience")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAddUpdateExperience");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<ExperienceDetails> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<ExperienceDetails>>(requestBody);

                return new OkObjectResult(_DoctorProfile.AddUpdateExperience(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToUpdateDrProfileBasicInfo")]
        public async Task<IActionResult> FuncForDrAppToUpdateDrProfileBasicInfo([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToUpdateDrProfileBasicInfo")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToUpdateDrProfileBasicInfo");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<BasicInfo> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<BasicInfo>>(requestBody);

                return new OkObjectResult(_DoctorProfile.UpdateDrProfileBasicInfo(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToSaveUpdateDrAwardsInfo")]
        public async Task<IActionResult> FuncForDrAppToSaveUpdateDrAwardsInfo([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToSaveUpdateDrAwardsInfo")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToSaveUpdateDrAwardsInfo");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<Awards> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<Awards>>(requestBody);

                return new OkObjectResult(_DoctorProfile.SaveUpdateDrAwardsInfo(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToDeleteDrAwardsInfo")]
        public async Task<IActionResult> FuncForDrAppToDeleteDrAwardsInfo([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToDeleteDrAwardsInfo")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToDeleteDrAwardsInfo");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<Awards> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<Awards>>(requestBody);

                return new OkObjectResult(_DoctorProfile.DeleteDrAwardsInfo(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToDeleteDrEducation")]
        public async Task<IActionResult> FuncForDrAppToDeleteDrEducation([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToDeleteDrEducation")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToDeleteDrEducation");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<EducationDetails> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<EducationDetails>>(requestBody);

                return new OkObjectResult(_DoctorProfile.DeleteDrEducation(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToDeleteDrExperience")]
        public async Task<IActionResult> FuncForDrAppToDeleteDrExperience([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToDeleteDrExperience")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToDeleteDrExperience");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<ExperienceDetails> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<ExperienceDetails>>(requestBody);

                return new OkObjectResult(_DoctorProfile.DeleteDrExperience(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }


        [FunctionName("FuncForDrAppToSearchSpecialisation")]
        public async Task<IActionResult> FuncForDrAppToSearchSpecialisation([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToSearchSpecialisation")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAddSpecialisation");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<SearchInput> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<SearchInput>>(requestBody);

                return new OkObjectResult(_DoctorProfile.SearchSpecialisation(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToAddSpecialisation")]
        public async Task<IActionResult> FuncForDrAppToAddSpecialisation([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAddSpecialisation")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAddSpecialisation");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<List<Specialisation>> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<List<Specialisation>>>(requestBody);

                return new OkObjectResult(_DoctorProfile.AddSpecialisation(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToSearchServices")]
        public async Task<IActionResult> FuncForDrAppToSearchServices([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToSearchServices")] HttpRequest req, ILogger log)
        {
            try
            {

                log.LogInformation("Inside FuncForDrAppToSearchServices");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }

                // pull data from HttpRequest Object and then put in calling UOW function, if required.

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();



                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);

                WrapperStandardInput<SearchInput> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<SearchInput>>(requestBody);



                return new OkObjectResult(_DoctorProfile.GetSearchServices(lInput));

            }

            catch (System.Exception ex)

            {

                throw ex;

            }

        }

        [FunctionName("FuncForDrAppToAddServices")]
        public async Task<IActionResult> FuncForDrAppToAddServices([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAddServices")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAddServices");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<List<UneecopsTechnologies.DronaDoctorApp.BAL.Dtos.Master.DoctorProfile.Services>> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<List<UneecopsTechnologies.DronaDoctorApp.BAL.Dtos.Master.DoctorProfile.Services>>>(requestBody);

                return new OkObjectResult(_DoctorProfile.AddServices(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppForShareLink")]
        public async Task<IActionResult> FuncForDrAppForShareLink([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppForShareLink")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppForShareLink");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<UneecopsTechnologies.DronaDoctorApp.BAL.Dtos.Master.DoctorProfile.SharelinkDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<UneecopsTechnologies.DronaDoctorApp.BAL.Dtos.Master.DoctorProfile.SharelinkDto>>(requestBody);

                return new OkObjectResult(_DoctorProfile.ShareLink(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
