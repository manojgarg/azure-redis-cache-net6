using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using UneecopsTechnologies.DronaDoctorApp.API.TokenAuth;
using UneecopsTechnologies.DronaDoctorApp.BAL.Base;
using UneecopsTechnologies.DronaDoctorApp.BAL.Dtos.Master.Community;
using UneecopsTechnologies.DronaDoctorApp.BAL.Interfaces.Master.Community;

namespace UneecopsTechnologies.DronaDoctorApp.API.Services.Master.Community
{
    public class CommunityService
    {
        private readonly IAccessTokenProvider _tokenProvider;
        private readonly ICommunity _Community;
        public CommunityService(IAccessTokenProvider tokenProvider, ICommunity Community)
        {
            this._tokenProvider = tokenProvider;
            this._Community = Community;
        }

        [FunctionName("FuncForDrAppToGetCommunityInfo")]
        public async Task<IActionResult> FuncForDrAppToGetCommunityInfo([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetCommunityInfo")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetCommunityInfo");
                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<CommunityDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<CommunityDto>>(requestBody);

                return new OkObjectResult(_Community.GetCommunityInfo(lInput));
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [FunctionName("FuncForDrAppToGetPostByTopics")]
        public async Task<IActionResult> FuncForDrAppToGetPostByTopics([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetPostByTopics")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetPostByTopics");
                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<CommunityDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<CommunityDto>>(requestBody);

                return new OkObjectResult(_Community.GetPostByTopics(lInput));
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [FunctionName("FuncForDrAppToGetPostById")]
        public async Task<IActionResult> FuncForDrAppToGetPostById([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetPostById")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetPostById");
                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<CommunityPost> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<CommunityPost>>(requestBody);

                return new OkObjectResult(_Community.GetPostById(lInput));
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [FunctionName("FuncForDrAppToSaveUpdateComment")]
        public async Task<IActionResult> FuncForDrAppToSaveUpdateComment([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToSaveUpdateComment")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToSaveUpdateComment");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }

                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<SaveCommentInputModel> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<SaveCommentInputModel>>(requestBody);

                return new OkObjectResult(_Community.SaveUpdateComment(lInput));
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [FunctionName("FuncForDrAppToToggleBookmarkPost")]
        public async Task<IActionResult> FuncForDrAppToToggleBookmarkPost([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToToggleBookmarkPost")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToToggleBookmarkPost");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }

                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<CommunityPost> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<CommunityPost>>(requestBody);

                return new OkObjectResult(_Community.ToggleBookmarkPost(lInput));
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [FunctionName("FuncForDrAppToJoinCommunity")]
        public async Task<IActionResult> FuncForDrAppToJoinCommunity([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToJoinCommunity")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToJoinCommunity");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }

                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<CommunityDto> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<CommunityDto>>(requestBody);

                return new OkObjectResult(_Community.JoinCommunity(lInput));
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [FunctionName("FuncForDrAppToSaveUpdateThanks")]
        public async Task<IActionResult> FuncForDrAppToSaveUpdateThanks([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToSaveUpdateThanks")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToSaveUpdateThanks");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }

                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<SaveThanksInputModel> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<SaveThanksInputModel>>(requestBody);

                return new OkObjectResult(_Community.SaveUpdateThanks(lInput));
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [FunctionName("FuncForDrAppToGetPostThanksById")]
        public async Task<IActionResult> FuncForDrAppToGetPostThanksById([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetPostThanksById")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetPostThanksById");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }

                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<GetThanksModel> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<GetThanksModel>>(requestBody);

                return new OkObjectResult(_Community.GetPostThanksById(lInput));
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [FunctionName("FuncForDrAppToSaveUpdateSharePost")]
        public async Task<IActionResult> FuncForDrAppToSaveUpdateSharePost([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToSaveUpdateSharePost")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToSaveUpdateSharePost");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }

                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<ShareInputModel> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<ShareInputModel>>(requestBody);

                return new OkObjectResult(_Community.SaveUpdateSharePost(lInput));
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [FunctionName("FuncForDrAppToGetPostShareDetails")]
        public async Task<IActionResult> FuncForDrAppToGetPostShareDetails([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetPostShareDetails")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetPostShareDetails");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }

                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<GetShareModel> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<GetShareModel>>(requestBody);

                return new OkObjectResult(_Community.GetPostShareDetails(lInput));
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [FunctionName("FuncForDrAppToPushUserPostView")]
        public async Task<IActionResult> FuncForDrAppToPushUserPostView([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToPushUserPostView")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToPushUserPostView");
                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<ShareInputModel> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<ShareInputModel>>(requestBody);

                return new OkObjectResult( _Community.PushUserPostView(lInput));
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [FunctionName("FuncForDrAppToPushUserReadPost")]
        public async Task<IActionResult> FuncForDrAppToPushUserReadPost([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToPushUserReadPost")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToPushUserReadPost");
                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }
                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<ShareInputModel> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<ShareInputModel>>(requestBody);

                return new OkObjectResult( _Community.PushUserReadPostk(lInput));
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }
        [FunctionName("FuncForDrAppToGetPostViewById")]
        public async Task<IActionResult> FuncForDrAppToGetPostViewById([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1/FuncForDrAppToGetPostViewById")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("Inside FuncForDrAppToGetPostViewById");

                var result = _tokenProvider.ValidateToken(req);

                if (!(result.Status == AccessTokenStatus.Valid))
                {
                    //return new OkObjectResult(new WrapperStandardOutput<string>("-1","Unauthorized"));
                    return new UnauthorizedResult();
                }

                // pull data from HttpRequest Object and then put in calling UOW function, if required.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                //dynamic dynamicObject = JsonConvert.DeserializeObject(requestBody);
                WrapperStandardInput<GetViewModel> lInput = JsonConvert.DeserializeObject<WrapperStandardInput<GetViewModel>>(requestBody);

                return new OkObjectResult(_Community.GetPostViewById(lInput));
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

    }
}
