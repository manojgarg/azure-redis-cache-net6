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
using UneecopsTechnologies.DronaDoctorApp.BAL.Dtos.Master.Records;
using UneecopsTechnologies.DronaDoctorApp.BAL.Interfaces.Master.Records;

namespace UneecopsTechnologies.DronaDoctorApp.API.Services.Master.Records
{
    public class RecordServices
    {
        private readonly IAccessTokenProvider _tokenProvider;
        private readonly IRecords _Records;
        public RecordServices(IAccessTokenProvider tokenProvider, IRecords Records)
        {
            this._tokenProvider = tokenProvider;
            this._Records = Records;
        }

        [FunctionName("FuncForDrAppToGetEditPatientRecord")]
        public async Task<IActionResult> FuncForDrAppToGetEditPatientRecord([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetEditPatientRecord")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetEditPatientRecord");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<PatientRecord> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PatientRecord>>(requestBody);
                return new OkObjectResult(_Records.GetPatientRecords(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToAddPatientRecord")]
        public async Task<IActionResult> FuncForDrAppToAddPatientRecord([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAddPatientRecord")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAddPatientRecord");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<PatientRecord> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PatientRecord>>(requestBody);
                return new OkObjectResult(_Records.AddPatientRecords(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception )
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToIsDoctorReadDocument")]
        public async Task<IActionResult> FuncForDrAppToIsDoctorReadDocument([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToIsDoctorReadDocument")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToIsDoctorReadDocument");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<PatientRecord> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PatientRecord>>(requestBody);
                return new OkObjectResult(_Records.IsDoctorReadDocument(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToGetFileInfo")]
        public async Task<IActionResult> FuncForDrAppToGetFileInfo([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetFileInfo")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetFileInfo");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<AttachmentFileInfo> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<AttachmentFileInfo>>(requestBody);
                return new OkObjectResult(_Records.GetFileInfo(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToIsDoctorDeleteRecord")]
        public async Task<IActionResult> FuncForDrAppToIsDoctorDeleteRecord([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToIsDoctorDeleteRecord")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToIsDoctorDeleteRecord");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<PatientRecord> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PatientRecord>>(requestBody);
                return new OkObjectResult(_Records.DeleteRecord(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }


        [FunctionName("FuncForDrAppToAddPatientRecord_V2")]
        public async Task<IActionResult> FuncForDrAppToAddPatientRecord_V2([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAddPatientRecord_V2")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAddPatientRecord_V2");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<PatientRecord> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PatientRecord>>(requestBody);
                return new OkObjectResult(_Records.AddPatientRecords_V2(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }


        [FunctionName("FuncForDrAppToGetFileInfo_V2")]
        public async Task<IActionResult> FuncForDrAppToGetFileInfo_V2([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetFileInfo_V2")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetFileInfo_V2");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<AttachmentFileInfo> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<AttachmentFileInfo>>(requestBody);
                return new OkObjectResult(_Records.GetFileInfo_V2(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
