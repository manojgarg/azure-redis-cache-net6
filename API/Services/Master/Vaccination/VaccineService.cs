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
using UneecopsTechnologies.DronaDoctorApp.BAL.Dtos.Master.Vaccination;
using UneecopsTechnologies.DronaDoctorApp.BAL.Dtos.Setting;
using UneecopsTechnologies.DronaDoctorApp.BAL.Interfaces.Master.Vaccination;

namespace UneecopsTechnologies.DronaDoctorApp.API.Services.Master.Vaccination
{
  public  class VaccineService
    {
        private readonly IAccessTokenProvider _tokenProvider;
        private readonly IVaccine _Vaccine;
        public VaccineService(IAccessTokenProvider tokenProvider, IVaccine Vaccine)
        {
            this._tokenProvider = tokenProvider;
            this._Vaccine = Vaccine;
        }
        [FunctionName("FuncForDrAppToGetVaccinationInfo")]
        public async Task<IActionResult> FuncForDrAppToGetVaccinationInfo([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetVaccinationInfo")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetVaccinationInfo");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    return new UnauthorizedResult();
                }

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<VaccinationDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<VaccinationDto>>(requestBody);
                return new OkObjectResult(_Vaccine.GetVaccinationInfo(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToSaveVaccinationInfo")]
        public async Task<IActionResult> FuncForDrAppToSaveVaccinationInfo([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToSaveVaccinationInfo")] HttpRequest req, ILogger log)
        {
            try
            {


                log.LogInformation("Inside FuncForDrAppToSaveVaccinationInfo");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    return new UnauthorizedResult();
                }

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<VaccinateDetails> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<VaccinateDetails>>(requestBody);
                return new OkObjectResult(_Vaccine.SaveVaccinationInfo(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToSavePregnencyInfo")]
        public async Task<IActionResult> FuncForDrAppToSavePregnencyInfo([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToSavePregnencyInfo")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToSavePregnencyInfo");



                var result = _tokenProvider.ValidateToken(req);

                //var result = _tokenProvider.ValidateToken(req);


                //if (!(result.Status == AccessTokenStatus.Valid))
                //{
                //    return new UnauthorizedResult();
                //}

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<PregnencyDetails> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PregnencyDetails>>(requestBody);
                return new OkObjectResult(_Vaccine.SavePregnencyInfo(lInput));

            }
            catch (Exception)
            {
                throw;
            }
        }
        [FunctionName("FuncForDrAppToGetPregnancyCalnderInfo")]
        public async Task<IActionResult> FuncForDrAppToGetPregnancyCalnderInfo([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetPregnancyCalnderInfo")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetPregnancyCalnderInfo");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    return new UnauthorizedResult();
                }

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<PregnencyDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PregnencyDto>>(requestBody);
                return new OkObjectResult(_Vaccine.GetPregnancyCalanderInfo(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToResetPregnancyCalnderInfo")]
        public async Task<IActionResult> FuncForDrAppToResetPregnancyCalnderInfo([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToResetPregnancyCalnderInfo")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToResetPregnancyCalnderInfo");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    return new UnauthorizedResult();
                }

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<PregnencyDetails> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PregnencyDetails>>(requestBody);
                return new OkObjectResult(_Vaccine.ResetPregnancyCalanderInfo(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
