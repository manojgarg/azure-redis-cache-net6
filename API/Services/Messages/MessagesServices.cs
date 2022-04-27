using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UneecopsTechnologies.DronaDoctorApp.BAL.Base;
using UneecopsTechnologies.DronaDoctorApp.API.TokenAuth;
using UneecopsTechnologies.DronaDoctorApp.BAL.Interfaces.Messages;
using UneecopsTechnologies.DronaDoctorApp.BAL.Dtos.Dashboard;
using UneecopsTechnologies.DronaDoctorApp.BAL.Common;

namespace UneecopsTechnologies.DronaDoctorApp.API.Services.Messages
{
    public class MessagesServices
    {
        private readonly IAccessTokenProvider _tokenProvider;
        private readonly IMessage _Message;
        public MessagesServices(IAccessTokenProvider tokenProvider, IMessage Message)
        {
            this._tokenProvider = tokenProvider;
            this._Message = Message;
        }

        [FunctionName("FuncForDrAppToGetMessageHistory")]
        public async Task<IActionResult> FuncForDrAppToGetMessageHistory([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetMessageHistory")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetMessageHistory");
                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<DeliveryReport> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<DeliveryReport>>(requestBody);

                return new OkObjectResult(_Message.GetMessageHistory(lInput));
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }
        [FunctionName("FuncForDrAppToGetContentTypeList")]
        public async Task<IActionResult> FuncForDrAppToGetContentTypeList([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetContentTypeList")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetContentTypeList");
                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<MessageFilterSortDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<MessageFilterSortDto>>(requestBody);

                return new OkObjectResult(await _Message.GetContentTypeList(lInput));
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }
        [FunctionName("FuncForDrAppToGetRecipientList")]
        public async Task<IActionResult> FuncForDrAppToGetRecipientList([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetRecipientList")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetRecipientList");
                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<MessageFilterSortDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<MessageFilterSortDto>>(requestBody);

                return new OkObjectResult(_Message.GetRecipientList(lInput));
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }
        [FunctionName("FuncForDrAppToSendMessageToAllRecipient")]
        public async Task<IActionResult> FuncForDrAppToSendMessageToAllRecipient([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToSendMessageToAllRecipient")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToSendMessageToAllRecipient");
                //var result = _tokenProvider.ValidateToken(req);

                //if (!(result.Status == AccessTokenStatus.Valid))
                //{
                //    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                //    return new UnauthorizedResult();
                //}
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<SendMessage> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<SendMessage>>(requestBody);

                return new OkObjectResult(_Message.SendMessageToAllRecipient(lInput));
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }
    }
}
