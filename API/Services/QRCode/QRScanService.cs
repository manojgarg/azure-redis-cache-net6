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
using UneecopsTechnologies.DronaDoctorApp.BAL.Dtos.Dashboard;
using UneecopsTechnologies.DronaDoctorApp.BAL.Dtos.QRCode;
using UneecopsTechnologies.DronaDoctorApp.BAL.Interfaces.QRCode;

namespace UneecopsTechnologies.DronaDoctorApp.API.Services.QRCode
{
    public class QRScanService
    {
        private readonly IAccessTokenProvider _tokenProvider;
        private readonly IQRScan _QRScan;
        public QRScanService(IAccessTokenProvider tokenProvider, IQRScan QRScan)
        {
            this._tokenProvider = tokenProvider;
            this._QRScan = QRScan;
        }
        [FunctionName("FuncForGetClinicDetailsForQRCode")]
        public async Task<IActionResult> FuncForGetClinicDetailsForQRCode([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForGetClinicDetailsForQRCode")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForGetClinicDetailsForQRCode");

                var result = _tokenProvider.ValidateToken(req);

                //if (!(result.Status == AccessTokenStatus.Valid))
                //{
                //    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                //    return new UnauthorizedResult();
                //}
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<ClinicDetails> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<ClinicDetails>>(requestBody);
                WrapperStandardOutput<ClinicDetails> lOutput = await _QRScan.GetClinicDetailsForQRCode(lInput);
                return new OkObjectResult(lOutput);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForQRUserToSendOTP")]
        public async Task<IActionResult> FuncForQRUserToSendOTP([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForQRUserToSendOTP")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForQRUserToSendOTP");
                var result = _tokenProvider.ValidateToken(req);
                //if (!(result.Status == AccessTokenStatus.Valid))
                //{
                //    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                //    return new UnauthorizedResult();
                //}

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<QRScanOtpDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<QRScanOtpDto>>(requestBody);

                return new OkObjectResult(_QRScan.SaveAndSendQRCodeOTP(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [FunctionName("FuncForQRUserToValidateOTP")]
        public async Task<IActionResult> FuncForQRUserToValidateOTP([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForQRUserToValidateOTP")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForQRUserToValidateOTP");
                var result = _tokenProvider.ValidateToken(req);

                //if (!(result.Status == AccessTokenStatus.Valid))
                //{
                //    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                //    return new UnauthorizedResult();
                //}
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<QRScanOtpDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<QRScanOtpDto>>(requestBody);

                var lOutput = _QRScan.ValidateQRCodeOTP(lInput);

                if (!String.IsNullOrEmpty(lOutput?.Data?.UserGuid) && lOutput.Data.IsOtpValid)
                {
                    lOutput.AccessToken = _tokenProvider.GenerateToken(lOutput.Data.UserGuid);
                }

                return new OkObjectResult(lOutput);

            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [FunctionName("FuncForQRCodeToGetPatientByMobile")]
        public async Task<IActionResult> FuncForQRCodeToGetPatientByMobile([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForQRCodeToGetPatientByMobile")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForQRCodeToGetPatientByMobile");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<PatientDetails> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PatientDetails>>(requestBody);

                return new OkObjectResult(_QRScan.GetQRCodePatientByMobile(lInput));
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [FunctionName("FuncForQRUserToGetGenderList")]
        public async Task<IActionResult> FuncForQRUserToGetGenderList([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForQRUserToGetGenderList")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForQRUserToGetGenderList");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<Gender> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<Gender>>(requestBody);

                return new OkObjectResult(await _QRScan.GetQRScanGendreList(lInput));
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [FunctionName("FuncForQRUserToGetRelationList")]
        public async Task<IActionResult> FuncForQRUserToGetRelationList([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForQRUserToGetRelationList")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForQRUserToGetRelationList");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<Gender> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<Gender>>(requestBody);

                return new OkObjectResult(await _QRScan.GetQRScanGendreRelationList(lInput));
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [FunctionName("FuncForQRScanToInsertUpdatePatientDetails")]
        public async Task<IActionResult> FuncForQRScanToInsertUpdatePatientDetails([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForQRScanToInsertUpdatePatientDetails")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForQRScanToInsertUpdatePatientDetails");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<PatientsDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PatientsDto>>(requestBody);

                return new OkObjectResult(_QRScan.QRScanInsertUpdatePatientDetails(lInput));
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

        [FunctionName("FuncForQRUserToGetPatientDetails")]
        public async Task<IActionResult> FuncForQRUserToGetPatientDetails([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForQRUserToGetPatientDetails")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForQRUserToGetPatientDetails");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<PatientDetails> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PatientDetails>>(requestBody);

                return new OkObjectResult(_QRScan.GetPatientDetailsForQRScan(lInput));
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

        [FunctionName("FuncForBookAppointmentByScanQRCode")]
        public async Task<IActionResult> FuncForBookAppointmentByScanQRCode([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForBookAppointmentByScanQRCode")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForBookAppointmentByScanQRCode");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<QRScanUserdto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<QRScanUserdto>>(requestBody);

                return new OkObjectResult(_QRScan.BookAppointmentByScanQRCode(lInput));
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }
        [FunctionName("FuncForQRCodeToBookedAppointmentCheckIn")]
        public async Task<IActionResult> FuncForQRCodeToBookedAppointmentCheckIn([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForQRCodeToBookedAppointmentCheckIn")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForQRCodeToBookedAppointmentCheckIn");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<QRScanOtpDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<QRScanOtpDto>>(requestBody);

                return new OkObjectResult(_QRScan.BookedAppointmentCheckInByQRCode(lInput));
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }
        [FunctionName("FuncForResetOrSaveGeneratedQRCode")]
        public async Task<IActionResult> FuncForResetOrSaveGeneratedQRCode([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForResetOrSaveGeneratedQRCode")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForResetOrSaveGeneratedQRCode");
                var result = _tokenProvider.ValidateToken(req);
                //if (!(result.Status == AccessTokenStatus.Valid))
                //{
                //    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                //    return new UnauthorizedResult();
                //}
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<GenerateQRCode> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<GenerateQRCode>>(requestBody);

                return new OkObjectResult(_QRScan.ResetOrSaveGeneratedQRCode(lInput));
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
