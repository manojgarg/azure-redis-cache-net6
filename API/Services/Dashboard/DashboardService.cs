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
using UneecopsTechnologies.DronaDoctorApp.BAL.Dtos.Dashboard;
using UneecopsTechnologies.DronaDoctorApp.BAL.Interfaces.Dashboard;

namespace UneecopsTechnologies.DronaDoctorApp.API.Services.Dashboard
{
    public class DashboardService
    {
        private readonly IAccessTokenProvider _tokenProvider;
        private readonly IDashboard _Dashboard;

        public DashboardService(IAccessTokenProvider tokenProvider, IDashboard Dashboard)
        {
            this._tokenProvider = tokenProvider;
            this._Dashboard = Dashboard;
        }

        [FunctionName("FuncForDrAppToCheckIfAlreadyRegistered")]
        public async Task<IActionResult> FuncForDrAppToCheckIfAlreadyRegistered([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToCheckIfAlreadyRegistered")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToCheckIfAlreadyRegistered");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }

                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<User> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<User>>(requestBody);

                var lOutput = _Dashboard.CheckIfUserAlreadyRegistered(lInput);

                if (!String.IsNullOrEmpty(lOutput?.Data?.UserGuid) && lOutput.Data.IsRegistered && lOutput.Data.IsActive && lInput.Data.IsGoogleSignIn)
                {
                    //lOutput.AccessToken = _tokenProvider.GenerateToken(lOutput.Data.UserGuid);

                    var input = JsonConvert.SerializeObject(lOutput).ToString();
                    WrapperStandardInput<HomeScreenComponent> hscInput = JsonConvert.DeserializeObject<WrapperStandardInput<HomeScreenComponent>>(input);
                    hscInput.Data = new HomeScreenComponent();
                    hscInput.Data.UserInfo = new User() { IsGoogleSignIn = lOutput.Data.IsGoogleSignIn };
                    hscInput.Data.DoctorInfo = new DoctorProfile();
                    hscInput.Data.ClinicDetailsList = new List<Clinicinfo>();
                    hscInput.Data.UserInfo.UserGuid = lOutput?.Data?.UserGuid;
                    hscInput.UserGuid = lOutput?.Data?.UserGuid;
                    var Output = _Dashboard.GetHomeScreenComponents(hscInput);
                    Output.AccessToken = _tokenProvider.GenerateToken(Output.Data.UserInfo.UserGuid);
                    return new OkObjectResult(Output);
                }
                else if (!String.IsNullOrEmpty(lOutput?.Data?.UserGuid) && lOutput.Data.IsRegistered && !lOutput.Data.IsActive)
                {
                    WrapperStandardInput<OtpMaster> otpInput = new WrapperStandardInput<OtpMaster>();
                    otpInput.Data = new OtpMaster();
                    otpInput.Data.UserGuid = lOutput.Data.UserGuid;
                    otpInput.Data.Reference = lOutput.Data.MobileNo;

                    WrapperStandardOutput<OtpMaster> otpOutput = _Dashboard.SaveAndSendOTP(otpInput);

                    lOutput.StatusCode = otpOutput.StatusCode;
                    lOutput.StatusMessage = otpOutput.StatusMessage;
                }

                return new OkObjectResult(lOutput);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToLoginWithPassword")]
        public async Task<IActionResult> FuncForDrAppToLoginWithPassword([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToLoginWithPassword")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToLoginWithPassword");

                var result = _tokenProvider.ValidateToken(req);

                //if (!(result.Status == AccessTokenStatus.Valid))
                //{
                //    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                //    return new UnauthorizedResult();
                //}
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<User> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<User>>(requestBody);
                var lOutput = _Dashboard.UserLoginWithPassword(lInput);

                if (lOutput?.Data?.UserGuid?.Length > 0)
                {
                    var input = JsonConvert.SerializeObject(lOutput).ToString();
                    if (lOutput?.Data?.IsActive == true)
                    {
                        WrapperStandardInput<HomeScreenComponent> hscInput = JsonConvert.DeserializeObject<WrapperStandardInput<HomeScreenComponent>>(input);
                        hscInput.Data = new HomeScreenComponent();
                        hscInput.Data.UserInfo = new User();
                        hscInput.Data.DoctorInfo = new DoctorProfile();
                        hscInput.Data.ClinicDetailsList = new List<Clinicinfo>();
                        hscInput.Data.UserInfo.UserGuid = lOutput?.Data?.UserGuid;
                        hscInput.UserGuid = lOutput?.Data?.UserGuid;
                        var Output = _Dashboard.GetHomeScreenComponents(hscInput);
                        Output.AccessToken = _tokenProvider.GenerateToken(lOutput?.Data?.UserGuid);
                        return new OkObjectResult(Output);
                    }

                }
                return new OkObjectResult(lOutput);

                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        //The name of the method is only indicative, do not use the word "Entity" in
        //actual methods 
        [FunctionName("FuncForDrAppToLoginWithPasswordEF")]
        public async Task<IActionResult> FuncForDrAppToLoginWithPasswordEntity([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToLoginWithPasswordEF")] HttpRequest req, ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            WrapperStandardInput<User> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<User>>(requestBody);

            var lOutput = _Dashboard.UserLoginWithPasswordEntity(lInput);

            if (lOutput?.Data?.UserGuid?.Length > 0)
            {
                var input = JsonConvert.SerializeObject(lOutput).ToString();
                if (lOutput?.Data?.IsActive == true)
                {
                    WrapperStandardInput<HomeScreenComponent> hscInput = JsonConvert.DeserializeObject<WrapperStandardInput<HomeScreenComponent>>(input);
                    hscInput.Data = new HomeScreenComponent();
                    hscInput.Data.UserInfo = new User();
                    hscInput.Data.DoctorInfo = new DoctorProfile();
                    hscInput.Data.ClinicDetailsList = new List<Clinicinfo>();
                    hscInput.Data.UserInfo.UserGuid = lOutput?.Data?.UserGuid;
                    hscInput.UserGuid = lOutput?.Data?.UserGuid;
                    var Output = _Dashboard.GetHomeScreenComponents(hscInput);
                    Output.AccessToken = _tokenProvider.GenerateToken(Output.Data.UserInfo.UserGuid);
                    return new OkObjectResult(Output);
                }

            }
            return new OkObjectResult(lOutput);
        }

        [FunctionName("FuncForDrAppToSendUserOTP")]
        public async Task<IActionResult> FuncForDrAppToSendUserOTP([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToSendUserOTP")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToSendUserOTP");
                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<GenOtp> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<GenOtp>>(requestBody);

                return new OkObjectResult(await _Dashboard.SendUserOTP(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToResetPassword")]
        public async Task<IActionResult> FuncForDrAppToResetPassword([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToResetPassword")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToResetPassword");
                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<User> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<User>>(requestBody);

                return new OkObjectResult(_Dashboard.UserPasswordReset(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        //    {
        //Data : {
        //    OtpGuid : "9ec7dcbd-f79d-4e78-ab45-95cf5f8d9a08",
        //    OtpValue : "1662",
        //    IsLoginWithOtp : true",
        //    UserGuid : "9ec7dcbd-f79d-4e78-ab45-95cf5f8d9a08"
        //      }
        //   }

        // This method is only for testing/POC purpose. Do not use this method
        /// <summary>
        /// This method is only for testing/POC purpose. Do not use this method
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("FuncForDrAppToValidateOTPCached")]
        public async Task<IActionResult> FuncForDrAppToValidateOTPCached([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToValidateOTPCached")] HttpRequest req, ILogger log)
        {
            log.LogInformation("Inside FuncForDrAppToValidateOTPCached");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            WrapperStandardInput<OtpMaster> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<OtpMaster>>(requestBody);
            var lOutput = _Dashboard.ValidateOTPCached(lInput).Result;
            return new OkObjectResult(lOutput);
        }



        [FunctionName("FuncForDrAppToValidateOTP")]
        public async Task<IActionResult> FuncForDrAppToValidateOTP([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToValidateOTP")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToValidateOTP");

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<OtpMaster> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<OtpMaster>>(requestBody);

                var lOutput = _Dashboard.ValidateOTP(lInput);

                if ((!String.IsNullOrEmpty(lOutput?.Data?.UserGuid) && lOutput.Data.RoleCode == 10 && lOutput.Data.IsLoginWithOtp && lOutput.Data.IsOtpValid && lOutput.Data.IsSubscribed)
                        || (!String.IsNullOrEmpty(lOutput?.Data?.UserGuid) && lOutput.Data.RoleCode == 70 && lOutput.Data.IsLoginWithOtp && lOutput.Data.IsOtpValid))
                {
                    //lOutput.AccessToken = _tokenProvider.GenerateToken(lOutput.Data.UserGuid);
                    var input = JsonConvert.SerializeObject(lOutput).ToString();
                    WrapperStandardInput<LoginComponentDto> hscInput = JsonConvert.DeserializeObject<WrapperStandardInput<LoginComponentDto>>(input);
                    hscInput.Data = new LoginComponentDto();
                    hscInput.Data.UserInfo = new User();
                    hscInput.Data.DoctorInfo = new DoctorProfile();
                    hscInput.Data.ClinicDetailsList = new List<Clinicinfo>();
                    hscInput.Data.UserInfo.UserGuid = lOutput?.Data?.UserGuid;
                    hscInput.UserGuid = lOutput?.Data?.UserGuid;
                    var Output = _Dashboard.GetLoginDetails(hscInput);
                    Output.Data.IsActive = lOutput.Data.IsActive;
                    Output.AccessToken = _tokenProvider.GenerateToken(Output.Data.UserInfo.UserGuid);
                    return new OkObjectResult(Output);
                }
                else if (!String.IsNullOrEmpty(lOutput?.Data?.UserGuid) && lOutput.Data.IsOtpValid)
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

        [FunctionName("FuncForDrAppToGetCityList")]
        public async Task<IActionResult> FuncForDrAppToGetCityList([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetCityList")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetCityList");
                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<CityDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<CityDto>>(requestBody);

                return new OkObjectResult(await _Dashboard.GetCityList(lInput));
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [FunctionName("FuncForDrAppToGetStateList")]
        public async Task<IActionResult> FuncForDrAppToGetStateList([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetStateList")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForWebDrAppToGetStateList");
                var result = _tokenProvider.ValidateToken(req);

                //if (!(result.Status == AccessTokenStatus.Valid))
                //{
                //    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                //    return new UnauthorizedResult();
                //}
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<StateDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<StateDto>>(requestBody);

                return new OkObjectResult(await _Dashboard.GetStateList(lInput));
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [FunctionName("FuncForDrAppToRegisterUser")]
        public async Task<IActionResult> FuncForDrAppToRegisterUser([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToRegisterUser")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToRegisterUser");
                var result = _tokenProvider.ValidateToken(req);

                //if (!(result.Status == AccessTokenStatus.Valid))
                //{
                //    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                //    return new UnauthorizedResult();
                //}
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                WrapperStandardInput<UserRegistration> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<UserRegistration>>(requestBody);

                var lOutput = _Dashboard.RegisterUser(lInput);

                if (!String.IsNullOrEmpty(lOutput?.Data?.UserModel?.UserGuid))
                {
                    WrapperStandardInput<OtpMaster> otpInput = new WrapperStandardInput<OtpMaster>();
                    otpInput.Data = new OtpMaster();
                    otpInput.Data.UserGuid = lOutput.Data.UserModel.UserGuid;
                    otpInput.Data.Reference = lOutput.Data.ProfileModel.MobileNo;

                    WrapperStandardOutput<OtpMaster> otpOutput = _Dashboard.SaveAndSendOTP(otpInput);

                    lOutput.StatusCode = otpOutput.StatusCode;
                    lOutput.StatusMessage = otpOutput.StatusMessage;
                }

                return new OkObjectResult(lOutput);
            }
            catch (Exception)
            {
                throw;
            }
        }

        // This method is only for POC/testing purpose to check OTP
        [FunctionName("FuncForDrAppToRegisterUserCached")]
        public async Task<IActionResult> FuncForDrAppToRegisterUserCached([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToRegisterUserCached")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToRegisterUserCached");

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                WrapperStandardInput<UserRegistration> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<UserRegistration>>(requestBody);

                WrapperStandardOutput<UserRegistration> lOutput = new WrapperStandardOutput<UserRegistration>() { StatusCode = "Success", StatusMessage = "Success" };

                if (!String.IsNullOrEmpty(lInput?.Data?.UserModel?.UserGuid))
                {
                    WrapperStandardInput<OtpMaster> otpInput = new WrapperStandardInput<OtpMaster>();
                    otpInput.Data = new OtpMaster();
                    otpInput.Data.UserGuid = lInput.Data.UserModel.UserGuid;
                    otpInput.Data.Reference = lInput.Data.ProfileModel.MobileNo;
                    otpInput.UserGuid = lInput.Data.UserModel.UserGuid;
                    WrapperStandardOutput<OtpMaster> otpOutput = _Dashboard.SaveInCacheAndSendOTP(otpInput).Result;
                }

                return new OkObjectResult(lOutput);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToSaveAndSendOTP")]
        public async Task<IActionResult> FuncForDrAppToSaveAndSendOTP([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToSaveAndSendOTP")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToSaveAndSendOTP");
                var result = _tokenProvider.ValidateToken(req);

                //if (!(result.Status == AccessTokenStatus.Valid))
                //{
                //    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                //    return new UnauthorizedResult();
                //}
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                WrapperStandardInput<OtpMaster> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<OtpMaster>>(requestBody);

                return new OkObjectResult(_Dashboard.SaveAndSendOTP(lInput));
                //return new OkObjectResult("Checking Success message");

            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToGetSpeciality")]
        public async Task<IActionResult> FuncForDrAppToGetSpeciality([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetSpeciality")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetSpeciality");
                var result = _tokenProvider.ValidateToken(req);

                //if (!(result.Status == AccessTokenStatus.Valid))
                //{
                //    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                //    return new UnauthorizedResult();
                //}
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<SpecialitiesDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<SpecialitiesDto>>(requestBody);

                return new OkObjectResult(await _Dashboard.GetSpeciality(lInput));
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [FunctionName("FuncForDrAppToGetRole")]
        public async Task<IActionResult> FuncForDrAppToGetRole([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetRole")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetRole");
                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<RoleDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<RoleDto>>(requestBody);

                return new OkObjectResult(await _Dashboard.GetRole(lInput));
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [FunctionName("FuncForDrAppToRegisterUserStep1")]
        public async Task<IActionResult> FuncForWebAppToRegisterUserStep1([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToRegisterUserStep1")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToRegisterUserStep1");

                var result = _tokenProvider.ValidateToken(req);

                //if (!(result.Status == AccessTokenStatus.Valid))
                //{
                //    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                //    return new UnauthorizedResult();
                //}
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<RegistrationStep1> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<RegistrationStep1>>(requestBody);

                return new OkObjectResult(_Dashboard.RegisterUserStep1(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToGetClinicList")]
        public async Task<IActionResult> FuncForDrAppToGetClinicList([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetClinicList")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetClinicList");
                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<ClinicMaster> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<ClinicMaster>>(requestBody);

                return new OkObjectResult(await _Dashboard.GetClinicList(lInput));
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [FunctionName("FuncForDrAppToRegisterUserClinic")]
        public async Task<IActionResult> FuncForDrAppToRegisterUserClinic([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToRegisterUserClinic")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToRegisterUserClinic");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<DoctorClinicModel> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<DoctorClinicModel>>(requestBody);
                WrapperStandardOutput<DoctorClinicModel> lOutput = _Dashboard.RegisterUserDocClinic(lInput);
                if (!String.IsNullOrEmpty(lOutput?.Data?.UserGuid) && lOutput.Data.IsActive)
                {
                    //lOutput.AccessToken = _tokenProvider.GenerateToken(lOutput.Data.UserGuid);

                    var input = JsonConvert.SerializeObject(lOutput).ToString();
                    WrapperStandardInput<HomeScreenComponent> hscInput = JsonConvert.DeserializeObject<WrapperStandardInput<HomeScreenComponent>>(input);
                    hscInput.Data = new HomeScreenComponent();
                    hscInput.Data.UserInfo = new User();
                    hscInput.Data.DoctorInfo = new DoctorProfile();
                    hscInput.Data.ClinicDetailsList = new List<Clinicinfo>();
                    hscInput.Data.UserInfo.UserGuid = lOutput?.Data?.UserGuid;
                    hscInput.UserGuid = lOutput?.Data?.UserGuid;
                    var Output = _Dashboard.GetHomeScreenComponents(hscInput);
                    Output.AccessToken = _tokenProvider.GenerateToken(Output.Data.UserInfo.UserGuid);
                    return new OkObjectResult(Output);
                }

                return new OkObjectResult(lOutput);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToAddClinicDetails")]
        public async Task<IActionResult> FuncForDrAppToAddClinicDetails([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAddClinicDetails")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAddClinicDetails");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<ClinicDetails> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<ClinicDetails>>(requestBody);


                return new OkObjectResult(_Dashboard.AddClinicDetails(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToGetClinicDetails")]
        public async Task<IActionResult> FuncForDrAppToGetClinicDetails([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetClinicDetails")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetClinicDetails");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<ClinicDetails> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<ClinicDetails>>(requestBody);
                WrapperStandardOutput<ClinicDetails> lOutput = await _Dashboard.GetDoctorClinicDetails(lInput);
                return new OkObjectResult(lOutput);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToSaveUpdateDoctorClinicTimings")]
        public async Task<IActionResult> FuncForDrAppToSaveUpdateDoctorClinicTimings([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToSaveUpdateDoctorClinicTimings")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToSaveUpdateDoctorClinicTimings");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<List<DoctorClinicTimings>> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<List<DoctorClinicTimings>>>(requestBody);

                return new OkObjectResult(_Dashboard.SaveUpdateDoctorClinicTimings(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToGetDoctorClinicTimings")]
        public async Task<IActionResult> FuncForDrAppToGetDoctorClinicTimings([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetDoctorClinicTimings")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetDoctorClinicTimings");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<DrClinicTimingInputModel> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<DrClinicTimingInputModel>>(requestBody);

                return new OkObjectResult(_Dashboard.GetDoctorClinicTimings(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToAddPatient")]
        public async Task<IActionResult> FuncForDrAppToAddPatient([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAddPatient")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAddPatient");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<PatientDetails> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PatientDetails>>(requestBody);


                return new OkObjectResult(_Dashboard.AddPatient(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToAddUpdateBank")]
        public async Task<IActionResult> FuncForDrAppToAddUpdateBank([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAddUpdateBank")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAddUpdateBank");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<BankMaster> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<BankMaster>>(requestBody);

                return new OkObjectResult(_Dashboard.AddUpdateBank(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToGetBankDetailsById")]
        public async Task<IActionResult> FuncForDrAppToGetBankDetailsById([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetBankDetailsById")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetBankDetailsById");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<BankMaster> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<BankMaster>>(requestBody);

                return new OkObjectResult(_Dashboard.GetBankDetailsById(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToGetBankList")]
        public async Task<IActionResult> FuncForDrAppToGetBankList([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetBankList")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetBankList");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<BankMaster> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<BankMaster>>(requestBody);

                return new OkObjectResult(_Dashboard.GetBankList(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToGetHomeScreenComponents")]
        public async Task<IActionResult> FuncForDrAppToGetHomeScreenComponents([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetHomeScreenComponents")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetHomeScreenComponents");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<HomeScreenComponent> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<HomeScreenComponent>>(requestBody);
                var Output = _Dashboard.GetHomeScreenComponents(lInput);
                Output.Data.UserInfo = new User();
                Output.Data.UserInfo.UserGuid = lInput.UserGuid;
                Output.AccessToken = _tokenProvider.GenerateToken(Output.Data.UserInfo.UserGuid);
                return new OkObjectResult(Output);
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToSetupClinicDetails")]
        public async Task<IActionResult> FuncForDrAppToSetupClinicDetails([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToSetupClinicDetails")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToSetupClinicDetails");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<ClinicSetup> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<ClinicSetup>>(requestBody);


                return new OkObjectResult(_Dashboard.SetupClinicDetails(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToGetAppointmentDateTime")]
        public async Task<IActionResult> FuncForDrAppToGetAppointmentDateTime([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetAppointmentDateTime")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetAppointmentDateTime");

                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();


                WrapperStandardInput<PatientCommonInfoDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PatientCommonInfoDto>>(requestBody);

                return new OkObjectResult(_Dashboard.GetAppointmentDateTime(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToSearchPatients")]
        public async Task<IActionResult> FuncForDrAppToSearchPatients([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToSearchPatients")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToSearchPatients");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<PatientMaster> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PatientMaster>>(requestBody);

                return new OkObjectResult(_Dashboard.SearchPatients(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToSearchPatientByMobile")]
        public async Task<IActionResult> FuncForDrAppToSearchPatientByMobile([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToSearchPatientByMobile")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToSearchPatientByMobile");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<PatientMaster> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PatientMaster>>(requestBody);

                return new OkObjectResult(_Dashboard.SearchPatientByMobile(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToGetPatientList")]
        public async Task<IActionResult> FuncForDrAppToGetPatientList([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetPatientList")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetPatientList");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<PatientListForAppointment> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PatientListForAppointment>>(requestBody);


                return new OkObjectResult(_Dashboard.GetPatientList(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToPatientBookAppointment")]
        public async Task<IActionResult> FuncForDrAppToPatientBookAppointment([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToPatientBookAppointment")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToPatientBookAppointment");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<PatientCommonInfoDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PatientCommonInfoDto>>(requestBody);


                return new OkObjectResult(_Dashboard.PatientBookAppointment(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToGetReasonOfVisitList")]
        public async Task<IActionResult> FuncForDrAppToGetReasonOfVisitList([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetReasonOfVisitList")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetReasonOfVisitList");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<ReasonOfVisit> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<ReasonOfVisit>>(requestBody);


                return new OkObjectResult(_Dashboard.GetReasonOfVisitList(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToGetAvailableTimeSlot")]
        public async Task<IActionResult> FuncForDrAppToGetAvailableTimeSlot([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetAvailableTimeSlot")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetAvailableTimeSlot");

                var result = _tokenProvider.ValidateToken(req);
                //if (!(result.Status == AccessTokenStatus.Valid))
                //{
                //    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                //    return new UnauthorizedResult();
                //}
                // pull data from HttpRequest Object and then put in calling UOW function, if required.

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();


                WrapperStandardInput<PatientCommonInfoDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PatientCommonInfoDto>>(requestBody);

                return new OkObjectResult(_Dashboard.GetAvailableTimeSlot(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToAddUpdateUpiDetails")]
        public async Task<IActionResult> FuncForDrAppToAddUpdateUpiDetails([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAddUpdateUpiDetails")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAddUpdateUpiDetails");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<UPIDetailsDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<UPIDetailsDto>>(requestBody);

                return new OkObjectResult(_Dashboard.AddUpdateUpi(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToGetHelpAndSupport")]
        public async Task<IActionResult> FuncForDrAppToGetHelpAndSupport([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetHelpAndSupport")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetHelpAndSupport");
                var result = _tokenProvider.ValidateToken(req);
                //if (!(result.Status == AccessTokenStatus.Valid))
                //{
                //    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                //    return new UnauthorizedResult();
                //}
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<CommonFilterSortDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<CommonFilterSortDto>>(requestBody);
                var Output = _Dashboard.HelpAndSupport(lInput);
                //Output.AccessToken = _tokenProvider.GenerateToken(Output.Data.UserInfo.UserGuid);
                return new OkObjectResult(Output);
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToAddUserReviewDetails")]
        public async Task<IActionResult> FuncForDrAppToAddUserReviewDetails([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAddUserReviewDetails")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAddUserReviewDetails");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<UserReviewDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<UserReviewDto>>(requestBody);

                return new OkObjectResult(_Dashboard.InsertUserReview(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncFoDrAppToGetRelationTypeList")]
        public async Task<IActionResult> FuncFoDrAppToGetRelationTypeList([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncFoDrAppToGetRelationTypeList")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncFoDrAppToGetRelationTypeList");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<RelationType> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<RelationType>>(requestBody);


                return new OkObjectResult(_Dashboard.GetRelationTypeDet(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        // The name of the method is only indicative, do not use the word "Cached" in actual methods 
        [FunctionName("FuncFoDrAppToGetRelationTypeListCached")]
        public async Task<IActionResult> FuncFoDrAppToGetRelationTypeListCached([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncFoDrAppToGetRelationTypeListCached")] HttpRequest req, ILogger log)
        {
            log.LogInformation("Inside FuncFoDrAppToGetRelationTypeListCached");
            AccessTokenResult result = _tokenProvider.ValidateToken(req);
            if (!(result.Status == AccessTokenStatus.Valid))
            {
                return new UnauthorizedResult();
            }
            // pull data from HttpRequest Object and then put in calling UOW function, if required.
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            return new OkObjectResult(_Dashboard.GetRelationTypes(requestBody).Result);
        }

        [FunctionName("FuncForDrAppToGetAddPatientList")]
        public async Task<IActionResult> FuncForDrAppToGetAddPatientList([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetAddPatientList")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetAddPatientList");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<PatientList> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PatientList>>(requestBody);

                return new OkObjectResult(_Dashboard.GetAddPatientList(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToGetNotificationList")]
        public async Task<IActionResult> FuncForDrAppToGetNotificationList([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetNotificationList")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetNotificationList");
                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<CommonFilterSortDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<CommonFilterSortDto>>(requestBody);

                return new OkObjectResult(await _Dashboard.GetNotificationList(lInput));
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [FunctionName("FuncForDrAppToMarkAllAsRead")]
        public async Task<IActionResult> FuncForDrAppToMarkAllAsRead([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToMarkAllAsRead")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToMarkAllAsRead");

                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<NotificationDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<NotificationDto>>(requestBody);
                var Output = _Dashboard.MarkAllAsRead(lInput);
                //Output.AccessToken = _tokenProvider.GenerateToken(Output.Data.UserInfo.UserGuid);
                return new OkObjectResult(Output);
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToLogOutUser")]
        public async Task<IActionResult> FuncForDrAppToLogOutUser([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToLogOutUser")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToLogOutUser");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<FcmModel> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<FcmModel>>(requestBody);

                return new OkObjectResult(_Dashboard.LogoutUser(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [FunctionName("FuncForDrAppToForgotPasswordWithOTP")]
        public async Task<IActionResult> FuncForDrAppToForgotPasswordWithOTP([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToForgotPasswordWithOTP")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToForgotPasswordWithOTP");
                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<OtpMaster> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<OtpMaster>>(requestBody);

                return new OkObjectResult(await _Dashboard.ForgotPassword(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToGetUserInfoByUserId")]
        public async Task<IActionResult> FuncForDrAppToGetUserInfoByUserId([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetUserInfoByUserId")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetUserInfoByUserId");
                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<UserInfo> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<UserInfo>>(requestBody);

                return new OkObjectResult(await _Dashboard.UserInfoByUserId(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToGetSearchSpecialisation")]
        public async Task<IActionResult> FuncForDrAppToGetSearchSpecialisation([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetSearchSpecialisation")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetSearchSpecialisation");
                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<CommonFilterSortDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<CommonFilterSortDto>>(requestBody);

                return new OkObjectResult(await _Dashboard.GetSearchSpecialisation(lInput));
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [FunctionName("FuncForDrAppToGetSearchServices")]
        public async Task<IActionResult> FuncForDrAppToGetSearchServices([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetSearchServices")] HttpRequest req, ILogger log)

        {

            try

            {

                log.LogInformation("Inside FuncForDrAppToGetSearchServices");

                var result = _tokenProvider.ValidateToken(req);



                if (!(result.Status == AccessTokenStatus.Valid))

                {

                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));

                    return new UnauthorizedResult();

                }

                // pull data from HttpRequest Object and then put in calling UOW function, if required.

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();



                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);

                WrapperStandardInput<CommonFilterSortDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<CommonFilterSortDto>>(requestBody);



                return new OkObjectResult(await _Dashboard.GetSearchServices(lInput));

            }

            catch (System.Exception ex)

            {

                throw ex;

            }

        }

        [FunctionName("FuncForDrAppToGetTag")]
        public async Task<IActionResult> FuncForDrAppToGetTag([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetTag")] HttpRequest req, ILogger log)

        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetTag");
                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))

                {

                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));

                    return new UnauthorizedResult();

                }

                // pull data from HttpRequest Object and then put in calling UOW function, if required.

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);

                WrapperStandardInput<PatientDetails> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PatientDetails>>(requestBody);

                return new OkObjectResult(_Dashboard.GetTag(lInput));
            }

            catch (System.Exception ex)

            {
                throw ex;

            }

        }

        [FunctionName("FuncForDrAppToAcceptAndDenyClinicRequest")]
        public async Task<IActionResult> FuncForDrAppToAcceptAndDenyClinicRequest([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAcceptAndDenyClinicRequest")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAcceptAndDenyClinicRequest");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<RecieveRequestDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<RecieveRequestDto>>(requestBody);

                return new OkObjectResult(_Dashboard.AcceptAndDenyClinicRequest(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [FunctionName("FuncForDrAppToGetRequestNotificationDetails")]
        public async Task<IActionResult> FuncForDrAppToGetRequestNotificationDetails([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetRequestNotificationDetails")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetRequestNotificationDetails");
                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<NotificationDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<NotificationDto>>(requestBody);

                return new OkObjectResult(await _Dashboard.GetRequestNotificationDetails(lInput));
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }
        [FunctionName("FuncForDrAppToGetHomeDashboardGraph")]
        public async Task<IActionResult> FuncForDrAppToGetHomeDashboardGraph([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetHomeDashboardGraph")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetHomeDashboardGraph");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<DashboardGraph> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<DashboardGraph>>(requestBody);
                WrapperStandardOutput<DashboardGraph> lOutput = await _Dashboard.GetHomeDashboardGraph(lInput);
                return new OkObjectResult(lOutput);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToGetPatientInQueue")]
        public async Task<IActionResult> FuncForDrAppToGetPatientInQueue([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetPatientInQueue")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetPatientInQueue");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<PatientInQueue> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PatientInQueue>>(requestBody);
                var Output = _Dashboard.GetPatientInQueue(lInput);
                return new OkObjectResult(Output);
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToGetPatientGroupedList")]
        public async Task<IActionResult> FuncForDrAppToGetPatientGroupedList([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetPatientGroupedList")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetPatientGroupedList");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<CombinePatientsList> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<CombinePatientsList>>(requestBody);

                return new OkObjectResult(_Dashboard.GetPatientGroupedList(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToGetAppointmentPatientDetails")]
        public async Task<IActionResult> FuncForDrAppToGetAppointmentPatientDetails([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetAppointmentPatientDetails")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetAppointmentPatientDetails");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<PatientAppointmentDetails> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PatientAppointmentDetails>>(requestBody);


                return new OkObjectResult(_Dashboard.PatientAppointmentDetails(lInput));
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToAddChiefComplaint")]
        public async Task<IActionResult> FuncForDrAppToAddChiefComplaint([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAddChiefComplaint")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAddChiefComplaint");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<ChiefComplaintDtos> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<ChiefComplaintDtos>>(requestBody);


                return new OkObjectResult(_Dashboard.AddChiefComplaint(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToAddPatientTag")]
        public async Task<IActionResult> FuncForDrAppToAddPatientTag([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAddPatientTag")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAddPatientTag");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<PatientTag> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PatientTag>>(requestBody);

                return new OkObjectResult(_Dashboard.AddPatientTag(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToRemovePatientTag")]
        public async Task<IActionResult> FuncForDrAppToRemovePatientTag([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToRemovePatientTag")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToRemovePatientTag");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<PatientTag> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PatientTag>>(requestBody);

                return new OkObjectResult(_Dashboard.RemovePatientTag(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToGetVisitInfo")]
        public async Task<IActionResult> FuncForDrAppToGetVisitInfo([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetVisitInfo")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetVisitInfo");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<VisitInfo> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<VisitInfo>>(requestBody);

                return new OkObjectResult(_Dashboard.GetVisitInfo(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }


        [FunctionName("FuncForDrAppToSearchForSymptom")]
        public async Task<IActionResult> FuncForDrAppToSearchForSymptom([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToSearchForSymptom")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToSearchForSymptom");

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

                return new OkObjectResult(_Dashboard.SearchForSymptom(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToSearchForFindings")]
        public async Task<IActionResult> FuncForDrAppToSearchForFindings([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToSearchForFindings")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToSearchForFindings");

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

                return new OkObjectResult(_Dashboard.SearchForFindings(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }


        [FunctionName("FuncForDrAppToSearchForDiagnosis")]
        public async Task<IActionResult> FuncForDrAppToSearchForDiagnosis([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToSearchForDiagnosis")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToSearchForDiagnosis");

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

                return new OkObjectResult(_Dashboard.SearchForDiagnosis(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToSearchForMedicine")]
        public async Task<IActionResult> FuncForDrAppToSearchForMedicine([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToSearchForMedicine")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToSearchForMedicine");

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

                return new OkObjectResult(_Dashboard.SearchForMedicine(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToSearchForInvestigation")]
        public async Task<IActionResult> FuncForDrAppToSearchForInvestigation([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToSearchForInvestigation")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToSearchForInvestigation");

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

                return new OkObjectResult(_Dashboard.SearchForInvestigation(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToSearchForInstructions")]
        public async Task<IActionResult> FuncForDrAppToSearchForInstructions([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToSearchForInstructions")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToSearchForInstructions");

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

                return new OkObjectResult(_Dashboard.SearchForInstructions(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToAddSymptoms")]
        public async Task<IActionResult> FuncForDrAppToAddSymptoms([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAddSymptoms")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAddSymptoms");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                //pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<Symptom> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<Symptom>>(requestBody);

                return new OkObjectResult(await _Dashboard.AddSymptoms(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToAddFinding")]
        public async Task<IActionResult> FuncForDrAppToAddFinding([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAddFinding")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAddFinding");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<Finding> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<Finding>>(requestBody);

                return new OkObjectResult(await _Dashboard.AddFinding(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToAddDiagnosis")]
        public async Task<IActionResult> FuncForDrAppToAddDiagnosis([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAddDiagnosis")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAddDiagnosis");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<Diagnosis> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<Diagnosis>>(requestBody);

                return new OkObjectResult(await _Dashboard.AddDiagnosis(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToRemoveSymptoms")]
        public async Task<IActionResult> FuncForDrAppToRemoveSymptoms([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToRemoveSymptoms")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToRemoveSymptoms");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<Symptom> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<Symptom>>(requestBody);

                return new OkObjectResult(await _Dashboard.RemoveSymptoms(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToRemoveFinding")]
        public async Task<IActionResult> FuncForDrAppToRemoveFinding([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToRemoveFinding")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToRemoveFinding");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<Finding> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<Finding>>(requestBody);

                return new OkObjectResult(await _Dashboard.RemoveFinding(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToRemoveDiagnosis")]
        public async Task<IActionResult> FuncForDrAppToRemoveDiagnosis([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToRemoveDiagnosis")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToRemoveDiagnosis");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<Diagnosis> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<Diagnosis>>(requestBody);

                return new OkObjectResult(await _Dashboard.RemoveDiagnosis(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }


        [FunctionName("FuncForDrAppToAddInvestigation")]
        public async Task<IActionResult> FuncForDrAppToAddInvestigation([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAddInvestigation")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAddInvestigation");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<Investigation> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<Investigation>>(requestBody);

                return new OkObjectResult(await _Dashboard.AddInvestigation(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToAddInstruction")]
        public async Task<IActionResult> FuncForDrAppToAddInstruction([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAddInstruction")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAddInstruction");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<Instructions> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<Instructions>>(requestBody);

                return new OkObjectResult(await _Dashboard.AddInstruction(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToAddPrescroptionNote")]
        public async Task<IActionResult> FuncForDrAppToAddPrescroptionNote([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAddPrescroptionNote")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAddPrescroptionNote");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<PrescriptionNote> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PrescriptionNote>>(requestBody);

                return new OkObjectResult(await _Dashboard.AddPrescriptionNote(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToRemoveInvestigation")]
        public async Task<IActionResult> FuncForDrAppToRemoveInvestigation([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToRemoveInvestigation")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToRemoveInvestigation");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<Investigation> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<Investigation>>(requestBody);

                return new OkObjectResult(await _Dashboard.RemoveInvestigation(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToRemoveInstruction")]
        public async Task<IActionResult> FuncForDrAppToRemoveInstruction([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToRemoveInstruction")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToRemoveInstruction");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<Instructions> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<Instructions>>(requestBody);

                return new OkObjectResult(await _Dashboard.RemoveInstruction(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToRemovePrescroptionNote")]
        public async Task<IActionResult> FuncForDrAppToRemovePrescroptionNote([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToRemovePrescroptionNote")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToRemovePrescroptionNote");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<PrescriptionNote> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PrescriptionNote>>(requestBody);

                return new OkObjectResult(await _Dashboard.RemovePrescriptionNote(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToAddUpdateMedicine")]
        public async Task<IActionResult> FuncForDrAppToAddUpdateMedicine([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAddUpdateMedicine")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAddUpdateMedicine");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<Medicine> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<Medicine>>(requestBody);

                return new OkObjectResult(_Dashboard.AddUpdateMedicine(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToRemoveMedicine")]
        public async Task<IActionResult> FuncForDrAppToRemoveMedicine([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToRemoveMedicine")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToRemoveMedicine");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<Medicine> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<Medicine>>(requestBody);

                return new OkObjectResult(await _Dashboard.RemoveMedicine(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }
        [FunctionName("FuncForDrAppToGetVitalsList")]
        public async Task<IActionResult> FuncForDrAppToGetVitalsList([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetVitalsList")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetVitalsList");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<VitalMaster> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<VitalMaster>>(requestBody);
                return new OkObjectResult(_Dashboard.GetVitalsList(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToVitalsCustomization")]
        public async Task<IActionResult> FuncForDrAppToVitalsCustomization([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToVitalsCustomization")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToVitalsCustomization");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<List<VitalMaster>> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<List<VitalMaster>>>(requestBody);

                return new OkObjectResult(_Dashboard.VitalsCustomization(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToPatientAppointmentVitalDetails")]
        public async Task<IActionResult> FuncForDrAppToPatientAppointmentVitalDetails([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToPatientAppointmentVitalDetails")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToPatientAppointmentVitalDetails");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<PatientAppointmentVitalDtos> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PatientAppointmentVitalDtos>>(requestBody);

                return new OkObjectResult(_Dashboard.GetPatientAppointmentVitalDetails(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }
        [FunctionName("FuncForDrAppToAddUpdatePatientVitals")]
        public async Task<IActionResult> FuncForDrAppToAddUpdatePatientVitals([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAddUpdatePatientVitals")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAddUpdatePatientVitals");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<PatientAppointmentVitalDtos> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PatientAppointmentVitalDtos>>(requestBody);


                return new OkObjectResult(_Dashboard.AddUpdatePatientAppointmentVitals(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToDoctorNote")]

        public async Task<IActionResult> FuncForDrAppToDoctorNote([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToDoctorNote")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToDoctorNote");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<DoctorNote> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<DoctorNote>>(requestBody);

                return new OkObjectResult(await _Dashboard.AddDoctorNote(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToAppointmentMarkAsNoShow")]
        public async Task<IActionResult> FuncForDrAppToAppointmentMarkAsNoShow([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAppointmentMarkAsNoShow")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAppointmentMarkAsNoShow");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data 3from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<ManagePatientAppointment> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<ManagePatientAppointment>>(requestBody);

                return new OkObjectResult(_Dashboard.AppointmentMarkAsNoShow(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToPatientBookAppointment_101")]
        public async Task<IActionResult> FuncForDrAppToPatientBookAppointment_101([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToPatientBookAppointment_101")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToPatientBookAppointment_101");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<PatientCommonInfoDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PatientCommonInfoDto>>(requestBody);


                return new OkObjectResult(_Dashboard.PatientBookAppointment_101(lInput));
            }
            catch (Exception)
            {
                throw;


            }
        }

        [FunctionName("FuncForDrAppToPatientCancelAppointment_101")]
        public async Task<IActionResult> FuncForDrAppToPatientCancelAppointment_101([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToPatientCancelAppointment_101")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToPatientCancelAppointment_101");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<PatientCommonInfoDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PatientCommonInfoDto>>(requestBody);


                return new OkObjectResult(_Dashboard.PatientCancelAppointment_101(lInput));
            }
            catch (Exception)
            {
                throw;


            }
        }

        [FunctionName("FuncForDrAppToCompleteConsultation")]
        public async Task<IActionResult> FuncForDrAppToCompleteConsultation([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToCompleteConsultation")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToCompleteConsultation");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<CompleteConsultation> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<CompleteConsultation>>(requestBody);

                return new OkObjectResult(_Dashboard.CompleteConsultation(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToConsulatationBillingPreview")]
        public async Task<IActionResult> FuncForDrAppToConsulatationBillingPreview([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToConsulatationBillingPreview")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToConsulatationBillingPreview");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<ConsultationBillingPreview> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<ConsultationBillingPreview>>(requestBody);


                return new OkObjectResult(_Dashboard.ConsulatationBillingPreview(lInput));
            }
            catch (Exception)
            {
                throw;


            }
        }

        [FunctionName("FuncForDrAppToAddPatientCondition")]
        public async Task<IActionResult> FuncForDrAppToAddPatientCondition([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAddPatientCondition")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAddPatientCondition");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<PatientCondition> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PatientCondition>>(requestBody);

                return new OkObjectResult(await _Dashboard.AddPatientCondition(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToRemovePatientCondition")]
        public async Task<IActionResult> FuncForDrAppToRemovePatientCondition([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToRemovePatientCondition")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToRemovePatientCondition");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<PatientCondition> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PatientCondition>>(requestBody);

                return new OkObjectResult(await _Dashboard.RemovePatientCondition(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToSearchForPatientCondition")]
        public async Task<IActionResult> FuncForDrAppToSearchForPatientCondition([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToSearchForPatientCondition")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToSearchForPatientCondition");

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

                return new OkObjectResult(_Dashboard.SearchForPatientCondition(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToAddPatientCurrentMedication")]
        public async Task<IActionResult> FuncForDrAppToAddPatientCurrentMedication([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAddPatientCurrentMedication")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAddPatientCurrentMedication");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<PatientCurrentMedication> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PatientCurrentMedication>>(requestBody);

                return new OkObjectResult(await _Dashboard.AddPatientCurrentMedication(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToRemovePatientCurrentMedication")]
        public async Task<IActionResult> FuncForDrAppToRemovePatientCurrentMedication([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToRemovePatientCurrentMedication")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToRemovePatientCurrentMedication");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<PatientCurrentMedication> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PatientCurrentMedication>>(requestBody);

                return new OkObjectResult(await _Dashboard.RemovePatientCurrentMedication(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToSearchForPatientCurrentMedication")]
        public async Task<IActionResult> FuncForDrAppToSearchForPatientCurrentMedication([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToSearchForPatientCurrentMedication")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToSearchForPatientCurrentMedication");

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

                return new OkObjectResult(_Dashboard.SearchForPatientCurrentMedication(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToSearchForAllergies")]
        public async Task<IActionResult> FuncForDrAppToSearchForAllergies([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToSearchForAllergies")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToSearchForAllergies");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data 3from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<SearchInput> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<SearchInput>>(requestBody);

                return new OkObjectResult(_Dashboard.SearchForAllergies(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToAddAllergies")]
        public async Task<IActionResult> FuncForDrAppToAddAllergies([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAddAllergies")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAddAllergies");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<Allergies> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<Allergies>>(requestBody);

                return new OkObjectResult(await _Dashboard.AddAllergies(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }
        [FunctionName("FuncForDrAppToAddAllergyReactions")]
        public async Task<IActionResult> FuncForDrAppToAddAllergyReactions([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAddAllergyReactions")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAddAllergyReactions");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<Allergies> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<Allergies>>(requestBody);

                return new OkObjectResult(await _Dashboard.AddAllergyReactions(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToRemoveAllergies")]
        public async Task<IActionResult> FuncForDrAppToRemoveAllergies([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToRemoveAllergies")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToRemoveAllergies");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<Allergies> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<Allergies>>(requestBody);

                return new OkObjectResult(await _Dashboard.RemoveAllergies(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToAddAllergySeverity")]
        public async Task<IActionResult> FuncForDrAppToAddAllergySeverity([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAddAllergySeverity")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAddAllergySeverity");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data 3from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<Allergies> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<Allergies>>(requestBody);

                return new OkObjectResult(await _Dashboard.AddAllergySeverity(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToAddAllergyNote")]
        public async Task<IActionResult> FuncForDrAppToAddAllergyNote([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAddAllergyNote")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAddAllergyNote");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data 3from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<Allergies> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<Allergies>>(requestBody);

                return new OkObjectResult(await _Dashboard.AddAllergyNote(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToAddPatientSignificantHistory")]
        public async Task<IActionResult> FuncForDrAppToAddPatientSignificantHistory([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAddPatientSignificantHistory")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAddPatientSignificantHistory");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data 3from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<PatientSignificantHistory> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PatientSignificantHistory>>(requestBody);

                return new OkObjectResult(await _Dashboard.AddPatientSignificantHistory(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToAddPatientFamilyHistory")]
        public async Task<IActionResult> FuncForDrAppToAddPatientFamilyHistory([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAddPatientFamilyHistory")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAddPatientFamilyHistory");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data 3from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<FamilyHistory> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<FamilyHistory>>(requestBody);

                return new OkObjectResult(await _Dashboard.AddPatientFamilyHistory(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToRemovePatientFamilyHistoryCondition")]
        public async Task<IActionResult> FuncForDrAppToRemovePatientFamilyHistoryCondition([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToRemovePatientFamilyHistoryCondition")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToRemovePatientFamilyHistoryCondition");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data 3from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<FamilyHistory> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<FamilyHistory>>(requestBody);

                return new OkObjectResult(await _Dashboard.RemovePatientFamilyHistoryCondition(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToChangePatientFamilyHistoryMember")]
        public async Task<IActionResult> FuncForDrAppToChangePatientFamilyHistoryMember([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToChangePatientFamilyHistoryMember")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToChangePatientFamilyHistoryMember");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data 3from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<FamilyHistory> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<FamilyHistory>>(requestBody);

                return new OkObjectResult(await _Dashboard.ChangePatientFamilyHistoryMember(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppForFamilyhistoryRowDelete")]
        public async Task<IActionResult> FuncForDrAppForFamilyhistoryRowDelete([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppForFamilyhistoryRowDelete")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppForFamilyhistoryRowDelete");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data 3from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<FamilyHistory> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<FamilyHistory>>(requestBody);

                return new OkObjectResult(_Dashboard.FamilyhistoryRowDelete(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }
        [FunctionName("FuncForDrAppToDeletePatient")]
        public async Task<IActionResult> FuncForDrAppToDeletePatient([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToDeletePatient")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToDeletePatient");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<PatientDetails> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PatientDetails>>(requestBody);


                return new OkObjectResult(await _Dashboard.DeletePatient(lInput));
            }
            catch (Exception)
            {
                throw;


            }
        }

        [FunctionName("FuncForDrAppToGetPatientDetailsById")]
        public async Task<IActionResult> FuncForDrAppToGetPatientDetailsById([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetPatientDetailsById")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetPatientDetailsById");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<PatientDetails> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PatientDetails>>(requestBody);

                return new OkObjectResult(_Dashboard.GetPatientDetailsById(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToAddTag")]
        public async Task<IActionResult> FuncForDrAppToAddTag([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAddTag")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAddTag");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<AddTag> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<AddTag>>(requestBody);

                return new OkObjectResult(_Dashboard.AddTag(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToRemoveCustomInstruction")]
        public async Task<IActionResult> FuncForDrAppToRemoveCustomInstruction([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToRemoveCustomInstruction")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToRemoveCustomInstruction");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<Instructions> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<Instructions>>(requestBody);

                return new OkObjectResult(await _Dashboard.RemoveCustomInstruction(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToRemoveCustomInvestigation")]
        public async Task<IActionResult> FuncForDrAppToRemoveCustomInvestigation([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToRemoveCustomInvestigation")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToRemoveCustomInvestigation");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<Investigation> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<Investigation>>(requestBody);

                return new OkObjectResult(await _Dashboard.RemoveCustomInvestigation(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }



        [FunctionName("FuncForDrAppToGetEditBillingItem")]
        public async Task<IActionResult> FuncForDrAppToGetEditBillingItem([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetEditBillingItem")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetEditBillingItem");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<BillingDetails> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<BillingDetails>>(requestBody);


                return new OkObjectResult(_Dashboard.GetBillingItem(lInput));
            }
            catch (Exception)
            {
                throw;


            }
        }

        [FunctionName("FuncForDrAppToAddConsulationBilling")]
        public async Task<IActionResult> FuncForDrAppToAddConsulationBilling([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAddConsulationBilling")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAddConsulationBilling");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<BillingDetails> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<BillingDetails>>(requestBody);


                return new OkObjectResult(_Dashboard.AddConsultationBilling(lInput));
            }
            catch (Exception)
            {
                throw;


            }
        }

        [FunctionName("FuncForDrAppToAddUpdateRecordPayment")]
        public async Task<IActionResult> FuncForDrAppToAddUpdateRecordPayment([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAddUpdateRecordPayment")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAddUpdateRecordPayment");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<BillingRecordPayment> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<BillingRecordPayment>>(requestBody);


                return new OkObjectResult(_Dashboard.AddUpdateRecordPayment(lInput));
            }
            catch (Exception)
            {
                throw;


            }
        }
        [FunctionName("FuncForDrAppToGetRecordPayment")]
        public async Task<IActionResult> FuncForDrAppToGetRecordPayment([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetRecordPayment")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetRecordPayment");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<BillingDetails> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<BillingDetails>>(requestBody);

                return new OkObjectResult(_Dashboard.GetRecordPayment(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToGetBillingReceipt")]
        public async Task<IActionResult> FuncForDrAppToGetBillingReceipt([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetBillingReceipt")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetBillingReceipt");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<BillingDetails> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<BillingDetails>>(requestBody);

                return new OkObjectResult(_Dashboard.GetBillingReceipt(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToCheckIfAlreadyRegistered_V2")]
        public async Task<IActionResult> FuncForDrAppToCheckIfAlreadyRegistered_V2([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V2/FuncForDrAppToCheckIfAlreadyRegistered")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToCheckIfAlreadyRegistered_V2");
                //var result = _tokenProvider.ValidateToken(req);

                var result = _tokenProvider.ValidateToken(req);


                //if (!(result.Status == AccessTokenStatus.Valid))
                //{
                //    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                //    return new UnauthorizedResult();
                //}
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<User> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<User>>(requestBody);

                var lOutput = _Dashboard.CheckIfUserAlreadyRegistered_V2(lInput);

                if (!String.IsNullOrEmpty(lOutput?.Data?.UserGuid) && lOutput.Data.IsRegistered && lOutput.Data.IsActive && lOutput.Data.IsSubscribed && lInput.Data.IsGoogleSignIn)
                {
                    //lOutput.AccessToken = _tokenProvider.GenerateToken(lOutput.Data.UserGuid);

                    var input = JsonConvert.SerializeObject(lOutput).ToString();
                    WrapperStandardInput<HomeScreenComponent> hscInput = JsonConvert.DeserializeObject<WrapperStandardInput<HomeScreenComponent>>(input);
                    hscInput.Data = new HomeScreenComponent();
                    hscInput.Data.UserInfo = new User() { IsGoogleSignIn = lOutput.Data.IsGoogleSignIn };
                    hscInput.Data.DoctorInfo = new DoctorProfile();
                    hscInput.Data.ClinicDetailsList = new List<Clinicinfo>();
                    hscInput.Data.UserInfo.UserGuid = lOutput?.Data?.UserGuid;
                    hscInput.UserGuid = lOutput?.Data?.UserGuid;
                    var Output = _Dashboard.GetHomeScreenComponents(hscInput);
                    Output.AccessToken = _tokenProvider.GenerateToken(Output.Data.UserInfo.UserGuid);
                    return new OkObjectResult(Output);
                }
                else if (!String.IsNullOrEmpty(lOutput?.Data?.UserGuid) && lOutput.Data.IsRegistered && (!lOutput.Data.IsActive || !lOutput.Data.IsSubscribed))
                {
                    WrapperStandardInput<OtpMaster> otpInput = new WrapperStandardInput<OtpMaster>();
                    otpInput.Data = new OtpMaster();
                    otpInput.Data.UserGuid = lOutput.Data.UserGuid;
                    otpInput.Data.Reference = lOutput.Data.MobileNo;

                    WrapperStandardOutput<OtpMaster> otpOutput = _Dashboard.SaveAndSendOTP(otpInput);

                    lOutput.StatusCode = otpOutput.StatusCode;
                    lOutput.StatusMessage = otpOutput.StatusMessage;
                }

                return new OkObjectResult(lOutput);

            }
            catch (Exception)
            {
                throw;
            }
        }
        [FunctionName("FuncForDrAppToGetBillingInfo")]
        public async Task<IActionResult> FuncForDrAppToGetBillingInfo([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetBillingInfo")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetBillingInfo");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<BillingInfo> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<BillingInfo>>(requestBody);

                return new OkObjectResult(_Dashboard.GetBillingInfo(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToValidateOTP_V2")]
        public async Task<IActionResult> FuncForDrAppToValidateOTP_V2([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToValidateOTP_V2")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToValidateOTP_V2");
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<OtpMaster> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<OtpMaster>>(requestBody);

                var lOutput = _Dashboard.ValidateOTP_V2(lInput);

                if (!String.IsNullOrEmpty(lOutput?.Data?.UserGuid) && lOutput.Data.IsLoginWithOtp && lOutput.Data.IsOtpValid && lOutput.Data.IsSubscribed)
                {
                    //lOutput.AccessToken = _tokenProvider.GenerateToken(lOutput.Data.UserGuid);
                    var input = JsonConvert.SerializeObject(lOutput).ToString();
                    WrapperStandardInput<HomeScreenComponent> hscInput = JsonConvert.DeserializeObject<WrapperStandardInput<HomeScreenComponent>>(input);
                    hscInput.Data = new HomeScreenComponent();
                    hscInput.Data.UserInfo = new User();
                    hscInput.Data.DoctorInfo = new DoctorProfile();
                    hscInput.Data.ClinicDetailsList = new List<Clinicinfo>();
                    hscInput.Data.UserInfo.UserGuid = lOutput?.Data?.UserGuid;
                    hscInput.UserGuid = lOutput?.Data?.UserGuid;
                    var Output = _Dashboard.GetHomeScreenComponents(hscInput);
                    Output.Data.IsActive = lOutput.Data.IsActive;
                    Output.AccessToken = _tokenProvider.GenerateToken(Output.Data.UserInfo.UserGuid);
                    return new OkObjectResult(Output);
                }
                else if (!String.IsNullOrEmpty(lOutput?.Data?.UserGuid) && lOutput.Data.IsOtpValid)
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

        [FunctionName("FuncForDrAppToGetActivationPackages")]
        public async Task<IActionResult> FuncForDrAppToGetActivationPackages([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetActivationPackages")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetActivationPackages");

                var result = _tokenProvider.ValidateToken(req);

                //if (!(result.Status == AccessTokenStatus.Valid))
                //{
                //    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                //    return new UnauthorizedResult();
                //}
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();




                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<CommonFilterSortDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<CommonFilterSortDto>>(requestBody);

                return new OkObjectResult(_Dashboard.GetActivationPackages(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToApplyActivationCode")]
        public async Task<IActionResult> FuncForDrAppToApplyActivationCode([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToApplyActivationCode")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToApplyActivationCode");

                var result = _tokenProvider.ValidateToken(req);

                //if (!(result.Status == AccessTokenStatus.Valid))
                //{
                //    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                //    return new UnauthorizedResult();
                //}
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<ActivationCode> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<ActivationCode>>(requestBody);

                return new OkObjectResult(_Dashboard.ApplyActivationCode(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToSaveSubscriptionDetails")]
        public async Task<IActionResult> FuncForDrAppToSaveSubscriptionDetails([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToSaveSubscriptionDetails")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToSaveSubscriptionDetails");

                var result = _tokenProvider.ValidateToken(req);

                //if (!(result.Status == AccessTokenStatus.Valid))
                //{
                //    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                //    return new UnauthorizedResult();
                //}
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<SubscriptionDetails> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<SubscriptionDetails>>(requestBody);

                return new OkObjectResult(_Dashboard.SaveSubscriptionDetails(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToCancelBilling")]
        public async Task<IActionResult> FuncForDrAppToCancelBilling([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToCancelBilling")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToCancelBilling");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<BillingDetails> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<BillingDetails>>(requestBody);

                return new OkObjectResult(_Dashboard.CancelBilling(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }
        [FunctionName("FuncForDrAppToGetEcardList")]
        public async Task<IActionResult> FuncForDrAppToGetEcardList([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetEcardList")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetEcardList");
                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<TopicCardDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<TopicCardDto>>(requestBody);

                return new OkObjectResult(_Dashboard.EcardList(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToGetEcardDetails")]
        public async Task<IActionResult> FuncForDrAppToGetEcardDetails([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetEcardDetails")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetEcardDetails");
                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<CardThumbnailDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<CardThumbnailDto>>(requestBody);

                return new OkObjectResult(_Dashboard.DoctorEcardDetails(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }


        [FunctionName("FuncForDrAppToSaveEcardDetails")]
        public async Task<IActionResult> FuncForDrAppToSaveEcardDetails([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToSaveEcardDetails")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToSaveEcardDetails");
                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<DoctorEcardDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<DoctorEcardDto>>(requestBody);

                return new OkObjectResult(_Dashboard.SaveDoctorEcardDetails(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToGetViewALLPastEncounter")]
        public async Task<IActionResult> FuncForDrAppToGetViewALLPastEncounter([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetViewALLPastEncounter")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetViewALLPastEncounter");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<PastEncounterlist> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PastEncounterlist>>(requestBody);

                return new OkObjectResult(_Dashboard.GetViewAllPastEncounter(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToRepeatVisit")]
        public async Task<IActionResult> FuncForDrAppToRepeatVisit([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToRepeatVisit")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToRepeatVisit");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<RepeatVisit> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<RepeatVisit>>(requestBody);

                return new OkObjectResult(_Dashboard.RepeatVisit(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToAddFollowUp")]
        public async Task<IActionResult> FuncForDrAppToAddFollowUp([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAddFollowUp")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAddFollowUp");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<FollowUp> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<FollowUp>>(requestBody);

                return new OkObjectResult(_Dashboard.AddFollowUp(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToGetDoctorClinicTimings_V2")]
        public async Task<IActionResult> FuncForDrAppToGetDoctorClinicTimings_V2([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetDoctorClinicTimings_V2")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetDoctorClinicTimings_V2");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<DrClinicTimingInputModel> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<DrClinicTimingInputModel>>(requestBody);

                return new OkObjectResult(_Dashboard.GetDoctorClinicTimings_V2(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToChangePassword")]
        public async Task<IActionResult> FuncForDrAppToChangePassword([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToChangePassword")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToChangePassword");
                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<ChangePassword> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<ChangePassword>>(requestBody);

                return new OkObjectResult(_Dashboard.UserChangePassword(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToGetAppointmentPatientDetails_V2")]
        public async Task<IActionResult> FuncForDrAppToGetAppointmentPatientDetails_V2([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetAppointmentPatientDetails_V2")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetAppointmentPatientDetails_V2");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<PatientAppointmentDetails> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PatientAppointmentDetails>>(requestBody);


                return new OkObjectResult(_Dashboard.PatientAppointmentDetails_V2(lInput));
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToSaveUpdateDoctorClinicTimings_V2")]
        public async Task<IActionResult> FuncForDrAppToSaveUpdateDoctorClinicTimings_V2([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToSaveUpdateDoctorClinicTimings_V2")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToSaveUpdateDoctorClinicTimings_V2");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<DoctorClinicTimings> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<DoctorClinicTimings>>(requestBody);

                return new OkObjectResult(_Dashboard.SaveUpdateDoctorClinicTimings_V2(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }


        [FunctionName("FuncForDrAppToAddUpdatePatientVitals_V2")]
        public async Task<IActionResult> FuncForDrAppToAddUpdatePatientVitals_V2([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAddUpdatePatientVitals_V2")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAddUpdatePatientVitals_V2");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<PatientAppointmentVitalDtos> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PatientAppointmentVitalDtos>>(requestBody);


                return new OkObjectResult(_Dashboard.AddUpdatePatientAppointmentVitals_V2(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToGetPatientGroupedList_V2")]
        public async Task<IActionResult> FuncForDrAppToGetPatientGroupedList_V2([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetPatientGroupedList_V2")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetPatientGroupedList_V2");

                //var result = _tokenProvider.ValidateToken(req);

                //if (!(result.Status == AccessTokenStatus.Valid))
                //{
                //    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                //    return new UnauthorizedResult();
                //}
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<CombinePatientsList> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<CombinePatientsList>>(requestBody);

                return new OkObjectResult(_Dashboard.GetPatientGroupedList_V2(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToGetNotificationList_V2")]
        public async Task<IActionResult> FuncForDrAppToGetNotificationList_V2([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetNotificationList_V2")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetNotificationList_V2");
                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<CommonFilterSortDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<CommonFilterSortDto>>(requestBody);

                return new OkObjectResult(_Dashboard.GetNotificationList_V2(lInput));
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }
        //[FunctionName("FuncForDrAppToCompleteConsultation_V2")]
        //public async Task<IActionResult> FuncForDrAppToCompleteConsultation_V2([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToCompleteConsultation_V2")] HttpRequest req, ILogger log)
        //{
        //    try
        //    {
        //        log.LogInformation("Inside FuncForDrAppToCompleteConsultation_V2");

        //        //var result = _tokenProvider.ValidateToken(req);

        //        //if (!(result.Status == AccessTokenStatus.Valid))
        //        //{
        //        //    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
        //        //    return new UnauthorizedResult();
        //        //}
        //        // pull data from HttpRequest Object and then put in calling UOW function, if required.
        //        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        //        WrapperStandardInput<CompleteConsultation> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<CompleteConsultation>>(requestBody);

        //        return new OkObjectResult(_Dashboard.CompleteConsultation_V2(lInput));
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}

        [FunctionName("FuncForDrAppToSaveBillingRecieptPdf")]
        public async Task<IActionResult> FuncForDrAppToSaveBillingRecieptPdf([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToSaveBillingRecieptPdf")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToSaveBillingRecieptPdf");


                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();



                WrapperStandardInput<PrintReciept> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PrintReciept>>(requestBody);

                return new OkObjectResult(_Dashboard.SaveBillingRecieptPdf(lInput));

                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToPatientBookAppointment_V2")]
        public async Task<IActionResult> FuncForDrAppToPatientBookAppointment_V2([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToPatientBookAppointment_V2")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToPatientBookAppointment_V2");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<PatientCommonInfoDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PatientCommonInfoDto>>(requestBody);


                return new OkObjectResult(_Dashboard.PatientBookAppointment_V2(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }
        [FunctionName("FuncForDrAppToPatientBookAppointment_101_V2")]
        public async Task<IActionResult> FuncForDrAppToPatientBookAppointment_101_V2([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToPatientBookAppointment_101_V2")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToPatientBookAppointment_101_V2");

                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    return new UnauthorizedResult();
                }
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                WrapperStandardInput<PatientCommonInfoDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PatientCommonInfoDto>>(requestBody);

                return new OkObjectResult(_Dashboard.PatientBookAppointment_101_V2(lInput));
            }
            catch (Exception)
            {
                throw;


            }
        }

        [FunctionName("FuncForDrAppToPatientCancelAppointment_101_V2")]
        public async Task<IActionResult> FuncForDrAppToPatientCancelAppointment_101_V2([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToPatientCancelAppointment_101_V2")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToPatientCancelAppointment_101_V2");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<PatientCommonInfoDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PatientCommonInfoDto>>(requestBody);


                return new OkObjectResult(_Dashboard.PatientCancelAppointment_101_V2(lInput));
            }
            catch (Exception)
            {
                throw;


            }
        }
        [FunctionName("FuncForDrAppToAddUpdateBank_V2")]
        public async Task<IActionResult> FuncForDrAppToAddUpdateBank_V2([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAddUpdateBank_V2")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAddUpdateBank_V2");

                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }

                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<BankMaster> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<BankMaster>>(requestBody);

                return new OkObjectResult(_Dashboard.AddUpdateBank_V2(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }
        [FunctionName("FuncForDrAppToSaveBillingRecieptPdf_V2")]
        public async Task<IActionResult> FuncForDrAppToSaveBillingRecieptPdf_V2([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToSaveBillingRecieptPdf_V2")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToSaveBillingRecieptPdf_V2");


                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }

                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();



                WrapperStandardInput<PrintReciept> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PrintReciept>>(requestBody);

                return new OkObjectResult(_Dashboard.SaveBillingRecieptPdf_V2(lInput));

                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToSetupClinicDetails_V2")]
        public async Task<IActionResult> FuncForDrAppToSetupClinicDetails_V2([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToSetupClinicDetails_V2")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToSetupClinicDetails_V2");

                //var result = _tokenProvider.ValidateToken(req);

                //if (!(result.Status == AccessTokenStatus.Valid))
                //{
                //    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                //    return new UnauthorizedResult();
                //}

                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<ClinicSetup> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<ClinicSetup>>(requestBody);


                return new OkObjectResult(_Dashboard.SetupClinicDetails_V2(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToGetHomeScreenComponents_V2")]

        public async Task<IActionResult> FuncForDrAppToGetHomeScreenComponents_V2([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetHomeScreenComponents_V2")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetHomeScreenComponents_V2");

                //var result = _tokenProvider.ValidateToken(req);
                //if (!(result.Status == AccessTokenStatus.Valid))
                //{
                //    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                //    return new UnauthorizedResult();
                //}

                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<HomeScreenComponent> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<HomeScreenComponent>>(requestBody);
                var Output = _Dashboard.GetHomeScreenComponents_V2(lInput);
                Output.Data.UserInfo = new User();
                Output.Data.UserInfo.UserGuid = lInput.UserGuid;
                Output.AccessToken = _tokenProvider.GenerateToken(Output.Data.UserInfo.UserGuid);
                return new OkObjectResult(Output);
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }


        [FunctionName("FuncForDrAppToCompleteConsultation_V2")]
        public async Task<IActionResult> FuncForWebAppToCompleteConsultation_V2([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToCompleteConsultation_V2")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToCompleteConsultation_V2");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }

                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<CompleteConsultation> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<CompleteConsultation>>(requestBody);

                return new OkObjectResult(_Dashboard.CompleteConsultation_V2(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToGetEcardList_V1")]
        public async Task<IActionResult> FuncForDrAppToGetEcardList_V1([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetEcardList_V1")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetEcardList_V1");
                var result = _tokenProvider.ValidateToken(req);

                //if (!(result.Status == AccessTokenStatus.Valid))
                //{
                //    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                //    return new UnauthorizedResult();
                //}
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<TopicCardDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<TopicCardDto>>(requestBody);

                return new OkObjectResult(_Dashboard.EcardList_V1(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToGetEcardDetails_V1")]
        public async Task<IActionResult> FuncForDrAppToGetEcardDetails_V1([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetEcardDetails_V1")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetEcardDetails_V1");
                var result = _tokenProvider.ValidateToken(req);

                //if (!(result.Status == AccessTokenStatus.Valid))
                //{
                //    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                //    return new UnauthorizedResult();
                //}
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<CardThumbnailDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<CardThumbnailDto>>(requestBody);

                return new OkObjectResult(_Dashboard.DoctorEcardDetails_V1(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }


        [FunctionName("FuncForDrAppToGetEditAssistanceDetails")]
        public async Task<IActionResult> FuncForDrAppToGetEditAssistanceDetails([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetEditAssistanceDetails")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetEditAssistanceDetails");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<AssistanceDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<AssistanceDto>>(requestBody);


                return new OkObjectResult(_Dashboard.GetEditAssistanceDetails(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToAddUpdateAssistanceDetails")]
        public async Task<IActionResult> FuncForDrAppToAddUpdateAssistanceDetails([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAddUpdateAssistanceDetails")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAddUpdateAssistanceDetails");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<AssistanceDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<AssistanceDto>>(requestBody);


                return new OkObjectResult(_Dashboard.AddUpdateAssistanceDetails(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToDeleteAssistanceDetails")]
        public async Task<IActionResult> FuncForDrAppToDeleteAssistanceDetails([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToDeleteAssistanceDetails")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToDeleteAssistanceDetails");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<AssistanceDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<AssistanceDto>>(requestBody);


                return new OkObjectResult(_Dashboard.DeleteAssistanceDetails(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }


        [FunctionName("FuncForDrAppToGetAssistantProfile")]
        public async Task<IActionResult> FuncForDrAppToGetAssistantProfile([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetAssistantProfile")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetAssistantProfile");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<AssistanceProfileDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<AssistanceProfileDto>>(requestBody);

                return new OkObjectResult(_Dashboard.GetAssistantProfile(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToUpdateAssistantProfile")]
        public async Task<IActionResult> FuncForWebAppToUpdateAssistantProfile([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToUpdateAssistantProfile")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToUpdateAssistantProfile");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<AssistanceProfileDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<AssistanceProfileDto>>(requestBody);

                return new OkObjectResult(_Dashboard.UpdateAssistantProfile(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }


        [FunctionName("FuncForDrAppToAssistantChangePassword")]
        public async Task<IActionResult> FuncForWebAppToAssistantChangePassword([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAssistantChangePassword")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAssistantChangePassword");
                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<ChangePassword> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<ChangePassword>>(requestBody);

                return new OkObjectResult(_Dashboard.AssistantChangePassword(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToCheckIfAlreadyRegistered_V3")]
        public async Task<IActionResult> FuncForDrAppToCheckIfAlreadyRegistered_V3([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToCheckIfAlreadyRegistered_V3")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToCheckIfAlreadyRegistered_V3");

                //var result = _tokenProvider.ValidateToken(req);
                //if (!(result.Status == AccessTokenStatus.Valid))
                //{
                //    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                //    return new UnauthorizedResult();
                //}
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<User> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<User>>(requestBody);

                var lOutput = _Dashboard.CheckIfUserAlreadyRegistered(lInput);

                if (!String.IsNullOrEmpty(lOutput?.Data?.UserGuid) && lOutput.Data.IsRegistered && lOutput.Data.IsActive && lOutput.Data.IsSubscribed && lInput.Data.IsGoogleSignIn)
                {
                    //lOutput.AccessToken = _tokenProvider.GenerateToken(lOutput.Data.UserGuid);

                    var input = JsonConvert.SerializeObject(lOutput).ToString();
                    WrapperStandardInput<LoginComponentDto> hscInput = JsonConvert.DeserializeObject<WrapperStandardInput<LoginComponentDto>>(input);
                    hscInput.Data = new LoginComponentDto();
                    hscInput.Data.UserInfo = new User() { IsGoogleSignIn = lOutput.Data.IsGoogleSignIn };
                    hscInput.Data.DoctorInfo = new DoctorProfile();
                    hscInput.Data.ClinicDetailsList = new List<Clinicinfo>();
                    hscInput.Data.UserInfo.UserGuid = lOutput?.Data?.UserGuid;
                    hscInput.UserGuid = lOutput?.Data?.UserGuid;
                    var Output = _Dashboard.GetLoginDetails(hscInput);
                    Output.AccessToken = _tokenProvider.GenerateToken(Output.Data.UserInfo.UserGuid);
                    return new OkObjectResult(Output);
                }
                else if ((!String.IsNullOrEmpty(lOutput?.Data?.UserGuid) && lOutput.Data.IsRegistered && (!lOutput.Data.IsActive || !lOutput.Data.IsSubscribed))
                        || (!String.IsNullOrEmpty(lOutput?.Data?.UserGuid) && lOutput.Data.RoleCode == 70 && lOutput.Data.IsFirstTimeLogin))
                {
                    WrapperStandardInput<OtpMaster> otpInput = new WrapperStandardInput<OtpMaster>();
                    otpInput.Data = new OtpMaster();
                    otpInput.Data.UserGuid = lOutput.Data.UserGuid;
                    otpInput.Data.Reference = lOutput.Data.MobileNo;

                    WrapperStandardOutput<OtpMaster> otpOutput = _Dashboard.SaveAndSendOTP(otpInput);

                    lOutput.StatusCode = otpOutput.StatusCode;
                    lOutput.StatusMessage = otpOutput.StatusMessage;
                }

                return new OkObjectResult(lOutput);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToLoginWithPassword_V2")]
        public async Task<IActionResult> FuncForDrAppToLoginWithPassword_V2([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToLoginWithPassword_V2")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToLoginWithPassword_V2");

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<User> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<User>>(requestBody);

                var lOutput = _Dashboard.UserLoginWithPassword(lInput);

                if (!String.IsNullOrEmpty(lOutput?.Data?.UserGuid) && lOutput.Data.IsActive)
                {
                    // lOutput.AccessToken = _tokenProvider.GenerateToken(lOutput.Data.UserGuid);

                    var input = JsonConvert.SerializeObject(lOutput).ToString();
                    WrapperStandardInput<LoginComponentDto> hscInput = JsonConvert.DeserializeObject<WrapperStandardInput<LoginComponentDto>>(input);
                    hscInput.Data = new LoginComponentDto();
                    hscInput.Data.UserInfo = new User() { IsGoogleSignIn = lOutput.Data.IsGoogleSignIn };
                    hscInput.Data.DoctorInfo = new DoctorProfile();
                    hscInput.Data.ClinicDetailsList = new List<Clinicinfo>();
                    hscInput.Data.UserInfo.UserGuid = lOutput?.Data?.UserGuid;
                    hscInput.UserGuid = lOutput?.Data?.UserGuid;
                    var Output = _Dashboard.GetLoginDetails(hscInput);
                    Output.AccessToken = _tokenProvider.GenerateToken(Output.Data.UserInfo.UserGuid);
                    return new OkObjectResult(Output);
                }

                return new OkObjectResult(lOutput);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToValidateOTP_V3")]
        public async Task<IActionResult> FuncForDrAppToValidateOTP_V3([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToValidateOTP_V3")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToValidateOTP_V3");
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<OtpMaster> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<OtpMaster>>(requestBody);

                var lOutput = _Dashboard.ValidateOTP_V2(lInput);
                var input = JsonConvert.SerializeObject(lOutput).ToString();
                if (!String.IsNullOrEmpty(lOutput?.Data?.UserGuid) && lOutput.Data.IsLoginWithOtp && lOutput.Data.IsOtpValid && lOutput.Data.IsSubscribed)
                {
                    //lOutput.AccessToken = _tokenProvider.GenerateToken(lOutput.Data.UserGuid);                    
                    WrapperStandardInput<LoginComponentDto> hscInput = JsonConvert.DeserializeObject<WrapperStandardInput<LoginComponentDto>>(input);
                    hscInput.Data = new LoginComponentDto();
                    hscInput.Data.UserInfo = new User();
                    hscInput.Data.DoctorInfo = new DoctorProfile();
                    hscInput.Data.ClinicDetailsList = new List<Clinicinfo>();
                    hscInput.Data.UserInfo.UserGuid = lOutput?.Data?.UserGuid;
                    hscInput.UserGuid = lOutput?.Data?.UserGuid;
                    var Output = _Dashboard.GetLoginDetails(hscInput);
                    Output.Data.IsActive = lOutput.Data.IsActive;
                    Output.AccessToken = _tokenProvider.GenerateToken(Output.Data.UserInfo.UserGuid);
                    return new OkObjectResult(Output);
                }
                else if (!String.IsNullOrEmpty(lOutput?.Data?.UserGuid) && lOutput.Data.IsOtpValid)
                {
                    WrapperStandardInput<LoginComponentDto> hscInput = JsonConvert.DeserializeObject<WrapperStandardInput<LoginComponentDto>>(input);
                    hscInput.Data = new LoginComponentDto();
                    hscInput.Data.UserInfo = new User();
                    hscInput.Data.DoctorInfo = new DoctorProfile();
                    hscInput.Data.ClinicDetailsList = new List<Clinicinfo>();
                    hscInput.Data.UserInfo.UserGuid = lOutput?.Data?.UserGuid;
                    var Output = _Dashboard.GetLoginDetails(hscInput);
                    Output.AccessToken = _tokenProvider.GenerateToken(lOutput.Data.UserGuid);
                    return new OkObjectResult(Output);

                }
                return new OkObjectResult(lOutput);

            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [FunctionName("FuncForDrAppToSignupAssistant")]
        public async Task<IActionResult> FuncForDrAppToSignupAssistant([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToSignupAssistant")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToSignupAssistant");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }

                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<User> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<User>>(requestBody);
                WrapperStandardInput<LoginComponentDto> newInput = new WrapperStandardInput<LoginComponentDto>();
                WrapperStandardOutput<LoginComponentDto> lOutput = new WrapperStandardOutput<LoginComponentDto>();
                lOutput.Data = new LoginComponentDto();

                WrapperStandardOutput<User> output = _Dashboard.UserPasswordReset(lInput);
                lOutput.Data.UserInfo = output?.Data;

                newInput.Data = lOutput.Data;

                if (!String.IsNullOrEmpty(lOutput?.Data?.UserInfo?.UserName))
                {
                    lOutput = _Dashboard.GetLoginDetails(newInput);
                    lOutput.AccessToken = _tokenProvider.GenerateToken(lOutput.Data.UserInfo.UserGuid);
                }

                return new OkObjectResult(lOutput);


            }
            catch (Exception ex)
            {
                throw;
            }
        }
        [FunctionName("FuncForDrAppToPatientBookAppointment_V3")]
        public async Task<IActionResult> FuncForDrAppToPatientBookAppointment_V3([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToPatientBookAppointment_V3")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToPatientBookAppointment_V3");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    return new UnauthorizedResult();
                }
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                WrapperStandardInput<PatientCommonInfoDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PatientCommonInfoDto>>(requestBody);

                return new OkObjectResult(_Dashboard.PatientBookAppointment_V3(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToPatientBookAppointment_V2_1")]
        public async Task<IActionResult> FuncForDrAppToPatientBookAppointment_V2_1([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToPatientBookAppointment_V2_1")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToPatientBookAppointment_V2_1");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<PatientCommonInfoDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PatientCommonInfoDto>>(requestBody);


                return new OkObjectResult(_Dashboard.PatientBookAppointment_V2_1(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToResetPassword_V1")]
        public async Task<IActionResult> FuncForDrAppToResetPassword_V1([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToResetPassword_V1")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToResetPassword_V1");
                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<User> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<User>>(requestBody);

                var IOutput = _Dashboard.UserPasswordReset(lInput);
                var input = JsonConvert.SerializeObject(IOutput).ToString();
                if (IOutput.Data.UserGuid != null)
                {
                    WrapperStandardInput<LoginComponentDto> hscInput = JsonConvert.DeserializeObject<WrapperStandardInput<LoginComponentDto>>(input);
                    hscInput.Data = new LoginComponentDto();
                    hscInput.Data.UserInfo = new User();
                    hscInput.Data.DoctorInfo = new DoctorProfile();
                    hscInput.Data.ClinicDetailsList = new List<Clinicinfo>();
                    hscInput.UserGuid = IOutput?.Data?.UserGuid;
                    hscInput.Data.UserInfo.UserGuid = IOutput?.Data?.UserGuid;
                    var Output = _Dashboard.GetLoginDetails(hscInput);
                    Output.AccessToken = _tokenProvider.GenerateToken(IOutput.Data.UserGuid);
                    return new OkObjectResult(Output);
                }
                return new OkObjectResult(IOutput);
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }
        [FunctionName("FuncForDrAppToValidateOTP_V4")]
        public async Task<IActionResult> FuncForDrAppToValidateOTP_V4([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToValidateOTP_V4")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToValidateOTP_V4");
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<OtpMaster> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<OtpMaster>>(requestBody);

                var lOutput = _Dashboard.ValidateOTP_V2(lInput);
                var input = JsonConvert.SerializeObject(lOutput).ToString();
                WrapperStandardOutput<LoginComponentDto> lOutputLogin = new WrapperStandardOutput<LoginComponentDto>();
                if (!String.IsNullOrEmpty(lOutput?.Data?.UserGuid) && lOutput.Data.IsLoginWithOtp && lOutput.Data.IsOtpValid && lOutput.Data.IsSubscribed)
                {
                    //lOutput.AccessToken = _tokenProvider.GenerateToken(lOutput.Data.UserGuid);                    
                    WrapperStandardInput<LoginComponentDto> hscInput = JsonConvert.DeserializeObject<WrapperStandardInput<LoginComponentDto>>(input);
                    hscInput.Data = new LoginComponentDto();
                    hscInput.Data.UserInfo = new User();
                    hscInput.Data.DoctorInfo = new DoctorProfile();
                    hscInput.Data.ClinicDetailsList = new List<Clinicinfo>();
                    hscInput.Data.UserInfo.UserGuid = lOutput?.Data?.UserGuid;
                    hscInput.UserGuid = lOutput?.Data?.UserGuid;
                    lOutputLogin = _Dashboard.GetLoginDetails(hscInput);
                    lOutputLogin.Data.IsActive = lOutputLogin.Data.IsActive;
                    lOutputLogin.AccessToken = _tokenProvider.GenerateToken(lOutputLogin.Data.UserInfo.UserGuid);
                    return new OkObjectResult(lOutputLogin);
                }
                else if (!String.IsNullOrEmpty(lOutput?.Data?.UserGuid) && lOutput.Data.IsOtpValid)
                {
                    WrapperStandardInput<LoginComponentDto> hscInput = JsonConvert.DeserializeObject<WrapperStandardInput<LoginComponentDto>>(input);
                    hscInput.Data = new LoginComponentDto();
                    hscInput.Data.UserInfo = new User();
                    hscInput.Data.DoctorInfo = new DoctorProfile();
                    hscInput.Data.ClinicDetailsList = new List<Clinicinfo>();
                    hscInput.Data.UserInfo.UserGuid = lInput.UserGuid;

                    var Output = _Dashboard.GetLoginDetails(hscInput);
                    Output.AccessToken = _tokenProvider.GenerateToken(lOutput.Data.UserGuid);
                    return new OkObjectResult(Output);

                }
                return new OkObjectResult(lOutput);

            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }


        [FunctionName("FuncForDrAppToCompleteConsultation_V3")]
        public async Task<IActionResult> FuncForWebAppToCompleteConsultation_V3([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToCompleteConsultation_V3")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToCompleteConsultation_V3");

                //var result = _tokenProvider.ValidateToken(req);

                //if (!(result.Status == AccessTokenStatus.Valid))
                //{
                //    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                //    return new UnauthorizedResult();
                //}

                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<CompleteConsultation> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<CompleteConsultation>>(requestBody);

                return new OkObjectResult(_Dashboard.CompleteConsultation_V3(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        [FunctionName("FuncForDrAppToSaveBillingRecieptPdf_V3")]
        public async Task<IActionResult> FuncForDrAppToSaveBillingRecieptPdf_V3([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToSaveBillingRecieptPdf_V3")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToSaveBillingRecieptPdf_V3");


                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }

                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();



                WrapperStandardInput<PrintReciept> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PrintReciept>>(requestBody);

                return new OkObjectResult(_Dashboard.SaveBillingRecieptPdf_V3(lInput));

                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        [FunctionName("FuncForDrAppToCheckIfAlreadyRegistered_V4")]
        public async Task<IActionResult> FuncForDrAppToCheckIfAlreadyRegistered_V4([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToCheckIfAlreadyRegistered_V4")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToCheckIfAlreadyRegistered_V4");

                //var result = _tokenProvider.ValidateToken(req);
                //if (!(result.Status == AccessTokenStatus.Valid))
                //{
                //    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                //    return new UnauthorizedResult();
                //}
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<User> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<User>>(requestBody);

                var lOutput = _Dashboard.CheckIfUserAlreadyRegistered_V4(lInput);

                if (!String.IsNullOrEmpty(lOutput?.Data?.UserGuid) && lOutput.Data.IsRegistered && lOutput.Data.IsActive && lOutput.Data.IsSubscribed && lInput.Data.IsGoogleSignIn)
                {
                    //lOutput.AccessToken = _tokenProvider.GenerateToken(lOutput.Data.UserGuid);

                    var input = JsonConvert.SerializeObject(lOutput).ToString();
                    WrapperStandardInput<LoginComponentDto> hscInput = JsonConvert.DeserializeObject<WrapperStandardInput<LoginComponentDto>>(input);
                    hscInput.Data = new LoginComponentDto();
                    hscInput.Data.UserInfo = new User() { IsGoogleSignIn = lOutput.Data.IsGoogleSignIn };
                    hscInput.Data.DoctorInfo = new DoctorProfile();
                    hscInput.Data.ClinicDetailsList = new List<Clinicinfo>();
                    hscInput.Data.UserInfo.UserGuid = lOutput?.Data?.UserGuid;
                    hscInput.UserGuid = lOutput?.Data?.UserGuid;
                    var Output = _Dashboard.GetLoginDetails(hscInput);
                    Output.AccessToken = _tokenProvider.GenerateToken(Output.Data.UserInfo.UserGuid);
                    return new OkObjectResult(Output);
                }
                else if ((!String.IsNullOrEmpty(lOutput?.Data?.UserGuid) && lOutput.Data.IsRegistered && (!lOutput.Data.IsActive || !lOutput.Data.IsSubscribed))
                        || (!String.IsNullOrEmpty(lOutput?.Data?.UserGuid) && lOutput.Data.RoleCode == 70 && lOutput.Data.IsFirstTimeLogin))
                {
                    WrapperStandardInput<OtpMaster> otpInput = new WrapperStandardInput<OtpMaster>();
                    otpInput.Data = new OtpMaster();
                    otpInput.Data.UserGuid = lOutput.Data.UserGuid;
                    otpInput.Data.Reference = lOutput.Data.MobileNo;

                    WrapperStandardOutput<OtpMaster> otpOutput = _Dashboard.SaveAndSendOTP(otpInput);

                    lOutput.StatusCode = otpOutput.StatusCode;
                    lOutput.StatusMessage = otpOutput.StatusMessage;
                }

                return new OkObjectResult(lOutput);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToRegisterUserStep1_V1")]
        public async Task<IActionResult> FuncForDrAppToRegisterUserStep1_V1([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToRegisterUserStep1_V1")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToRegisterUserStep1_V1");

                var result = _tokenProvider.ValidateToken(req);

                //if (!(result.Status == AccessTokenStatus.Valid))
                //{
                //    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                //    return new UnauthorizedResult();
                //}
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<RegistrationStep1> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<RegistrationStep1>>(requestBody);

                return new OkObjectResult(_Dashboard.RegisterUserStep1_V1(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }



        [FunctionName("FuncForDrAppToAddStep1ClinicDetails")]
        public async Task<IActionResult> FuncForDrAppToAddStep1ClinicDetails([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAddStep1ClinicDetails")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAddStep1ClinicDetails");

                var result = _tokenProvider.ValidateToken(req);

                //if (!(result.Status == AccessTokenStatus.Valid))
                //{
                //    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                //    return new UnauthorizedResult();
                //}
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<ClinicDetailOnBroading> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<ClinicDetailOnBroading>>(requestBody);
                var lOutput = _Dashboard.AddOnBoardingStep1ClinicDetails(lInput);

                //if (!String.IsNullOrEmpty(lOutput?.Data?.UserGuid) )
                //{
                //    //lOutput.AccessToken = _tokenProvider.GenerateToken(lOutput.Data.UserGuid);

                //    var input = JsonConvert.SerializeObject(lOutput).ToString();
                //    WrapperStandardInput<LoginComponentDto> hscInput = JsonConvert.DeserializeObject<WrapperStandardInput<LoginComponentDto>>(input);
                //    hscInput.Data = new LoginComponentDto();
                //    hscInput.Data.UserInfo = new User();
                //    hscInput.Data.DoctorInfo = new DoctorProfile();
                //    hscInput.Data.ClinicDetailsList = new List<Clinicinfo>();
                //    hscInput.Data.UserInfo.UserGuid = lOutput?.Data?.UserGuid;
                //    hscInput.UserGuid = lOutput?.Data?.UserGuid;
                //    var Output = _Dashboard.GetLoginDetails_V4(hscInput);
                //    Output.AccessToken = _tokenProvider.GenerateToken(Output.Data.UserInfo.UserGuid);
                //    Output.StatusCode = lOutput.StatusCode;
                //    Output.StatusMessage = lOutput.StatusMessage;
                //    return new OkObjectResult(Output);
                //}
                return new OkObjectResult(lOutput);
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception ex)
            {
                throw;
            }
        }



        [FunctionName("FuncForDrAppToGetClinicDetailsStep1")]
        public async Task<IActionResult> FuncForDrAppToGetClinicDetailsStep1([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetClinicDetailsStep1")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetClinicDetailsStep1");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<ClinicDetailOnBroading> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<ClinicDetailOnBroading>>(requestBody);


                return new OkObjectResult(_Dashboard.GetClinicDetailsStep1_V1(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToGetClinicAccessMasterStep1")]
        public async Task<IActionResult> FuncForDrAppToGetClinicAccessMasterStep1([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetClinicAccessMasterStep1")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetClinicAccessMasterStep1");

                var result = _tokenProvider.ValidateToken(req);

                //if (!(result.Status == AccessTokenStatus.Valid))
                //{
                //    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                //    return new UnauthorizedResult();
                //}
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<StaffAccessMaster> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<StaffAccessMaster>>(requestBody);


                return new OkObjectResult(_Dashboard.GetClinicDetailsAccessMasterStep1(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        #region  left menu list

        [FunctionName("FuncForDrAppToGetHomeScreenComponents_V3")]

        public async Task<IActionResult> FuncForDrAppToGetHomeScreenComponents_V3([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetHomeScreenComponents_V3")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetHomeScreenComponents_V3");

                //var result = _tokenProvider.ValidateToken(req);
                //if (!(result.Status == AccessTokenStatus.Valid))
                //{
                //    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                //    return new UnauthorizedResult();
                //}

                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<HomeScreenComponent> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<HomeScreenComponent>>(requestBody);
                var Output = _Dashboard.GetHomeScreenComponents_V3(lInput);
                //Output.Data.UserInfo = new User();
                //Output.Data.UserInfo.UserGuid = lInput.UserGuid;
                Output.AccessToken = _tokenProvider.GenerateToken(Output.Data.UserInfo.UserGuid);
                return new OkObjectResult(Output);
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToOnBoardingPriviewDoctorInfo")]
        public async Task<IActionResult> FuncForDrAppToOnBoardingPriviewDoctorInfo([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToOnBoardingPriviewDoctorInfo")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToOnBoardingPriviewDoctorInfo");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<OnBoardingPriviewInfo> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<OnBoardingPriviewInfo>>(requestBody);

                return new OkObjectResult(_Dashboard.OnBoardingPriviewDoctorInfo(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region resend pyment link

        [FunctionName("FuncForDrAppResendPymentLink")]
        public async Task<IActionResult> FuncForDrAppResendPymentLink([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppResendPymentLink")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppResendPymentLink");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                WrapperStandardInput<PatientCommonInfoDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PatientCommonInfoDto>>(requestBody);

                return new OkObjectResult(_Dashboard.PatientResendSmsMailPaymentLink(lInput));
            }
            catch (Exception)
            {
                throw;


            }
        }

        #endregion

        [FunctionName("FuncForDrAppToSaveUpdateDoctorConsultationTimings")]
        public async Task<IActionResult> FuncForDrAppToSaveUpdateDoctorConsultationTimings([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToSaveUpdateDoctorConsultationTimings")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToSaveUpdateDoctorConsultationTimings");

                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }

                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<ConsultationTimingDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<ConsultationTimingDto>>(requestBody);

                return new OkObjectResult(_Dashboard.SaveUpdateDoctorConsultationTimings(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToGetDoctorConsultationTimings")]
        public async Task<IActionResult> FuncForDrAppToGetDoctorConsultationTimings([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetDoctorConsultationTimings")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetDoctorConsultationTimings");

                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }

                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<ConsultationTimingDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<ConsultationTimingDto>>(requestBody);

                return new OkObjectResult(_Dashboard.GetDoctorConsultationTimings(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToAddDoctorESignature")]
        public async Task<IActionResult> FuncForDrAppToAddDoctorESignature([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAddDoctorESignature")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAddDoctorESignature");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    // return new OkObjectResult(new WrapperStandardOutput<string>("-1", "Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<Esignatue> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<Esignatue>>(requestBody);
                return new OkObjectResult(_Dashboard.AddDoctorESignature(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForDrAppToGetDoctorESignature")]
        public async Task<IActionResult> FuncForDrAppToGetDoctorESignature([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetDoctorESignature")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetDoctorESignature");
                var result = _tokenProvider.ValidateToken(req);
                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    // return new OkObjectResult(new WrapperStandardOutput<string>("-1", "Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<DoctorESignature> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<DoctorESignature>>(requestBody);
                return new OkObjectResult(_Dashboard.GetDoctorESignature(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [FunctionName("FuncForWebAppToGetRunTimeAppointmentDateTime")]
        public async Task<IActionResult> FuncForWebAppToGetRunTimeAppointmentDateTime([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForWebAppToGetRunTimeAppointmentDateTime")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForWebAppToGetRunTimeAppointmentDateTime");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<PatientCommonInfoDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<PatientCommonInfoDto>>(requestBody);

                return new OkObjectResult(_Dashboard.GetRunTimeAppointmentDateTime(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }
        [FunctionName("FuncForDrAppToRemoveDoctorESignature")]
        public async Task<IActionResult> FuncForDrAppToRemoveDoctorESignature([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToRemoveDoctorESignature")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToRemoveDoctorESignature");
                //var result = _tokenProvider.ValidateToken(req);
                //if (!(result.Status == AccessTokenStatus.Valid))
                //{
                //   // return new OkObjectResult(new WrapperStandardOutput<string>("-1", "Unauthorized"));
                //    return new UnauthorizedResult();
                //}
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<DoctorESignature> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<DoctorESignature>>(requestBody);
                return new OkObjectResult(_Dashboard.RemoveDoctorESignature(lInput));
                //return new OkObjectResult("Checking Success message");
            }
            catch (Exception)
            {
                throw;
            }
        }
        [FunctionName("FuncForDrAppToGetMedicineType")] //Medicine type get api
        public async Task<IActionResult> FuncForDrAppToGetMedicineType([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetMedicineType")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetMedicineType");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<MedicineTypeDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<MedicineTypeDto>>(requestBody);

                return new OkObjectResult(_Dashboard.GetMedicineType(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }
        [FunctionName("FuncForDrAppToAddNewMedicine")] //Add new custom medicine
        public async Task<IActionResult> FuncForDrAppToAddNewMedicine([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToAddNewMedicine")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToAddNewMedicine");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<NewMedicine> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<NewMedicine>>(requestBody);

                return new OkObjectResult(_Dashboard.AddNewMedicine(lInput));
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region handwritten Prescription
        [FunctionName("FuncForDrAppToCompleteConsultation_V4")]
        public async Task<IActionResult> FuncForDrAppToCompleteConsultation_V4([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToCompleteConsultation_V4")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToCompleteConsultation_V4");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    return new UnauthorizedResult();
                }

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<CompleteConsultation> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<CompleteConsultation>>(requestBody);

                return new OkObjectResult(_Dashboard.CompleteConsultation_V4(lInput));

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #endregion

        #region handwritten Prescription doctor private note
        [FunctionName("FuncForDrAppToCompleteConsultation_V5")]
        public async Task<IActionResult> FuncForDrAppToCompleteConsultation_V5([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToCompleteConsultation_V5")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToCompleteConsultation_V5");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    return new UnauthorizedResult();
                }

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                WrapperStandardInput<CompleteConsultation> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<CompleteConsultation>>(requestBody);

                return new OkObjectResult(_Dashboard.CompleteConsultation_V5(lInput));

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #endregion

    }
}
