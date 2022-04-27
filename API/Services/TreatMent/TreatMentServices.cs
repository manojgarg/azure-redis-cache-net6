using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UneecopsTechnologies.DronaDoctorApp.API.TokenAuth;
using UneecopsTechnologies.DronaDoctorApp.BAL.Base;
using UneecopsTechnologies.DronaDoctorApp.BAL.Dtos.TreatMent;
using UneecopsTechnologies.DronaDoctorApp.BAL.Interfaces.Treatment;

namespace UneecopsTechnologies.DronaDoctorApp.API.Services.TreatMent
{
    public class TreatMentServices
    {
        private readonly IAccessTokenProvider _tokenProvider;
        private readonly ITreatMentAPI _ITreatMentAPI;

        public TreatMentServices(IAccessTokenProvider tokenProvider, ITreatMentAPI TreatMentAPI)
        {
            this._tokenProvider = tokenProvider;
            this._ITreatMentAPI = TreatMentAPI;
        }

        #region Treatment Plan
        [FunctionName("FuncForDrAppToGetTreatmentList")]
        public async Task<IActionResult> FuncForDrAppToGetTreatmentList([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetTreatmentList")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetTreatmentList");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    return new OkObjectResult(new WrapperStandardOutput<string>("-1", "Unauthorized"));

                }
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                WrapperStandardInput<TreatMentModel> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<TreatMentModel>>(requestBody);

                return new OkObjectResult(_ITreatMentAPI.GetTreatmentList(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToAddNewTreatment")]
        public async Task<IActionResult> FuncForDrAppToAddNewTreatment([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAddNewTreatment")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAddNewTreatment");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    return new OkObjectResult(new WrapperStandardOutput<string>("-1", "Unauthorized"));

                }
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                WrapperStandardInput<TreatMentModel> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<TreatMentModel>>(requestBody);

                return new OkObjectResult(_ITreatMentAPI.AddNewTreatMent(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToAddTreatmentDetails")]
        public async Task<IActionResult> FuncForDrAppToAddTreatmentDetails([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAddTreatmentDetails")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAddTreatmentDetails");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    return new OkObjectResult(new WrapperStandardOutput<string>("-1", "Unauthorized"));

                }
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                WrapperStandardInput<TreatmentDetailsModel> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<TreatmentDetailsModel>>(requestBody);

                return new OkObjectResult(_ITreatMentAPI.AddTreatMentDetails(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        //[FunctionName("FuncForDrAppToGetTreatmentDetails")]
        //public async Task<IActionResult> FuncForDrAppToGetTreatmentDetails([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetTreatmentDetails")] HttpRequest req, ILogger log)
        //{
        //    try
        //    {
        //        log.LogInformation("Inside FuncForDrAppToGetTreatmentDetails");
        //        var result = _tokenProvider.ValidateToken(req);
        //        if (!(result.Status == AccessTokenStatus.Valid))
        //        {
        //            return new OkObjectResult(new WrapperStandardOutput<string>("-1", "Unauthorized"));

        //        }
        //        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        //        WrapperStandardInput<TreatMentModel> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<TreatMentModel>>(requestBody);

        //        return new OkObjectResult(_ITreatMentAPI.AddNewTreatMent(lInput));
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        [FunctionName("FuncForDrAppToMarkTreatmentAsComplete")]
        public async Task<IActionResult> FuncForDrAppToMarkTreatmentAsComplete([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToMarkTreatmentAsComplete")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToMarkTreatmentAsComplete");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    return new OkObjectResult(new WrapperStandardOutput<string>("-1", "Unauthorized"));

                }
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                WrapperStandardInput<TreatmentDetailsModel> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<TreatmentDetailsModel>>(requestBody);

                return new OkObjectResult(_ITreatMentAPI.MarkTreatmentAsComplete(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToGetTreatmentDetails")]
        public async Task<IActionResult> FuncForDrAppToGetTreatmentDetails([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetTreatmentDetails")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetTreatmentDetails");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    return new OkObjectResult(new WrapperStandardOutput<string>("-1", "Unauthorized"));

                }
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                WrapperStandardInput<TreatMentModel> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<TreatMentModel>>(requestBody);

                return new OkObjectResult(_ITreatMentAPI.GetTreatMentDetails(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToAddTreatmentAppointment")]
        public async Task<IActionResult> FuncForDrAppToAddTreatmentAppointment([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAddTreatmentAppointment")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAddTreatmentAppointment");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    return new OkObjectResult(new WrapperStandardOutput<string>("-1", "Unauthorized"));

                }
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                WrapperStandardInput<TreatmentAppointment> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<TreatmentAppointment>>(requestBody);

                return new OkObjectResult(_ITreatMentAPI.AddTreatmentAppointment(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion


        #region Doctor Private Note
        [FunctionName("FuncForDrAppToAddDoctorPrivateNote")]
        public async Task<IActionResult> FuncForDrAppToAddDoctorPrivateNote([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAddDoctorPrivateNote")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAddDoctorPrivateNote");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    return new OkObjectResult(new WrapperStandardOutput<string>("-1", "Unauthorized"));

                }
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                WrapperStandardInput<DoctorPrivateNotes> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<DoctorPrivateNotes>>(requestBody);

                return new OkObjectResult(_ITreatMentAPI.AddDoctorPrivateNote(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToGetDoctorPrivateNote")]
        public async Task<IActionResult> FuncForDrAppToGetDoctorPrivateNote([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetDoctorPrivateNote")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetDoctorPrivateNote");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    return new OkObjectResult(new WrapperStandardOutput<string>("-1", "Unauthorized"));

                }
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                WrapperStandardInput<DoctorPrivateNotes> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<DoctorPrivateNotes>>(requestBody);

                return new OkObjectResult(_ITreatMentAPI.GetDoctorPrivateNote(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

    }
}
