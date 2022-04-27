 namespace UneecopsTechnologies.DronaDoctorApp.API.TokenAuth
{
    using Microsoft.AspNetCore.Http;
    using UneecopsTechnologies.DronaDoctorApp.BAL.Base;
    using UneecopsTechnologies.DronaDoctorApp.BAL.Dtos;
    using UneecopsTechnologies.DronaDoctorApp.DAL.Dtos;

    /// <summary>
    /// Validates access tokes that have been submitted as part of a request.
    /// </summary>
    public interface IAccessTokenProvider
    {
        /// <summary>
        /// Validate the access token, returning the security principal in a result.
        /// </summary>
        /// <param name="request">The HTTP request containing the access token.</param>
        /// <returns>A result that contains the security principal.</returns>
        AccessTokenResult ValidateToken(HttpRequest request);
        WrapperStandardOutput<ExecuteDMLDto> ValidateService(WrapperStandardInput<LoginDto> aInput);
        string GenerateToken(string Guid);
    }
}
