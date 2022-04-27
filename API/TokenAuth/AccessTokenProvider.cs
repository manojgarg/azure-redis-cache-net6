namespace UneecopsTechnologies.DronaDoctorApp.API.TokenAuth
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;
    using Microsoft.AspNetCore.Http;
    using Microsoft.IdentityModel.Tokens;
    using Newtonsoft.Json;
    using UneecopsTechnologies.DronaDoctorApp.BAL.Base;
    using UneecopsTechnologies.DronaDoctorApp.BAL.Dtos;
    using UneecopsTechnologies.DronaDoctorApp.DAL.DBFunctions;
    using UneecopsTechnologies.DronaDoctorApp.DAL.Dtos;
    using UneecopsTechnologies.DronaDoctorApp.DAL.Errors;

    /// <summary>
    /// Validates a incoming request and extracts any <see cref="ClaimsPrincipal"/> contained within the bearer token.
    /// </summary>
    public class AccessTokenProvider : IAccessTokenProvider
    {
        private const string AUTH_HEADER_NAME = "Authorization";
        private const string BEARER_PREFIX = "Bearer ";
        private readonly string _issuerToken;
        private readonly string _audience;
        private readonly string _issuer;
        private readonly int _expiryHours;
        private readonly int _serviceExpiryHours;

        public AccessTokenProvider(string issuerToken, string audience, string issuer,int expiryHours, int serviceExpiryHours )
        {
            _issuerToken = issuerToken;
            _audience = audience;
            _issuer = issuer;
            _expiryHours = expiryHours;
            _serviceExpiryHours = serviceExpiryHours;
        }

        public AccessTokenResult ValidateToken(HttpRequest request)
        {
            try
            {
                // Get the token from the header
                if (request != null &&
                    request.Headers.ContainsKey(AUTH_HEADER_NAME) &&
                    request.Headers[AUTH_HEADER_NAME].ToString().StartsWith(BEARER_PREFIX))
                {
                    var token = request.Headers[AUTH_HEADER_NAME].ToString().Substring(BEARER_PREFIX.Length);
                    // Create the parameters
                    var tokenParams = new TokenValidationParameters()
                    {
                        RequireSignedTokens = true,
                       // ValidAudience = _audience,
                        ValidateAudience = false,
                      //  ValidIssuer = _issuer,
                        ValidateIssuer = false,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_issuerToken))
                    };

                    // Validate the token
                    var handler = new JwtSecurityTokenHandler();
                    var result = handler.ValidateToken(token, tokenParams, out var securityToken);
                    return AccessTokenResult.Success(result);
                }
                else
                {
                    return AccessTokenResult.NoToken();
                }
            }
            catch (SecurityTokenExpiredException)
            {
                return AccessTokenResult.Expired();
            }
            catch (Exception ex)
            {
                return AccessTokenResult.Error(ex);
            }
        }


        public WrapperStandardOutput<ExecuteDMLDto> ValidateService(WrapperStandardInput<LoginDto> aInput)
        {
            WrapperStandardOutput<ExecuteDMLDto> lOutput = new WrapperStandardOutput<ExecuteDMLDto>();

            try
            {
                // Call a proc and send all teh Item Group data in it, inside the proc check if itemgroup exists, if yes, then update thise rows, if not then insert those rows.

                 lOutput.Data = new DBFunctionsMySQLJson().ExecuteDML("USPForClientSvcAppToValidateService" + aInput.Version, JsonConvert.SerializeObject(aInput).ToString());

                return lOutput;

            }
            catch (Exception)
            {
                throw;
            }
        }

        public string GenerateToken(string guid)
        {
            string token = null;
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_issuerToken);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, guid)
                    }),
                    Expires = DateTime.UtcNow.AddHours(_expiryHours),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                                            SecurityAlgorithms.HmacSha256Signature)
                };
                var tempToken = tokenHandler.CreateToken(tokenDescriptor);
                return new JwtSecurityTokenHandler().WriteToken(tempToken);

            }
            catch (Exception ex)
            {
                ErrorLogs.WriteErrorLog(ex, "Utility : GenerateToken");
      
            }
            return token;
        }
    }
}
