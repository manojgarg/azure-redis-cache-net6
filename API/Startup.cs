using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using UneecopsTechnologies.DronaDoctorApp.API.TokenAuth;
using UneecopsTechnologies.DronaDoctorApp.BAL.Interfaces;
using UneecopsTechnologies.DronaDoctorApp.BAL.Uows;
using UneecopsTechnologies.DronaDoctorApp.BAL.Interfaces.Dashboard;
using UneecopsTechnologies.DronaDoctorApp.BAL.Uows.Dashboard;
using UneecopsTechnologies.DronaDoctorApp.BAL.Interfaces.Master.AboutUs;
using UneecopsTechnologies.DronaDoctorApp.BAL.Uows.Master.AboutUs;
using UneecopsTechnologies.DronaDoctorApp.BAL.Interfaces.Master.DoctorProfile;
using UneecopsTechnologies.DronaDoctorApp.BAL.Uows.Master.DoctorProfile;
using UneecopsTechnologies.DronaDoctorApp.BAL.Interfaces.Master.Community;
using UneecopsTechnologies.DronaDoctorApp.BAL.Uows.Master.Community;
using UneecopsTechnologies.DronaDoctorApp.BAL.Interfaces.Master.VideoCalling;
using UneecopsTechnologies.DronaDoctorApp.BAL.Uows.Master.VideoCalling;
using UneecopsTechnologies.DronaDoctorApp.BAL.Interfaces.OtherServices.Twilio;
using UneecopsTechnologies.DronaDoctorApp.BAL.Uows.OtherServices.Twilio;
using UneecopsTechnologies.DronaDoctorApp.BAL.Dtos.OtherServices.Twilio;
using UneecopsTechnologies.DronaDoctorApp.BAL.Interfaces.Master.Records;
using UneecopsTechnologies.DronaDoctorApp.BAL.Uows.Master.Records;
using UneecopsTechnologies.DronaDoctorApp.BAL.Interfaces.Setting;
using UneecopsTechnologies.DronaDoctorApp.BAL.Uows.Setting;
using UneecopsTechnologies.DronaDoctorApp.BAL.Interfaces.Master.HomeAnalytics;
using UneecopsTechnologies.DronaDoctorApp.BAL.Uows.Master.HomeAnalytics;
using UneecopsTechnologies.DronaDoctorApp.BAL.Uows.Messages;
using UneecopsTechnologies.DronaDoctorApp.BAL.Interfaces.Messages;
using UneecopsTechnologies.DronaDoctorApp.BAL.Interfaces.QRCode;
using UneecopsTechnologies.DronaDoctorApp.BAL.Uows.QRCode;
using UneecopsTechnologies.DronaDoctorApp.BAL.Interfaces.Master.Vaccination;
using UneecopsTechnologies.DronaDoctorApp.BAL.Uows.Master.Vaccination;
using UneecopsTechnologies.DronaDoctorApp.BAL.Interfaces.Treatment;
using UneecopsTechnologies.DronaDoctorApp.BAL.Uows.TreatMent;
using UneecopsTechnologies.DronaDoctorApp.DAL.DBFunctions;
using UneecopsTechnologies.DronaDoctorApp.Persistance.Interfaces;
using Microsoft.EntityFrameworkCore;
using UneecopsTechnologies.DronaDoctorApp.Persistance;
using UneecopsTechnologies.DronaDoctorApp.Cache;
using UneecopsTechnologies.DronaDoctorApp.Cache.Interfaces;
using UneecopsTechnologies.DronaDoctorApp.Cache.Implementations;
using System.Configuration;

[assembly: FunctionsStartup(typeof(UneecopsTechnologies.DronaDoctorApp.API.Startup))]
namespace UneecopsTechnologies.DronaDoctorApp.API
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        //public void Configure(IWebJobsBuilder builder)
            {
            //Get Jwt Token Issuer From Config
            var issuerToken = DBFunctionsMySQLBase.configSettings["Values:IssuerToken"];
            var audience = DBFunctionsMySQLBase.configSettings["Values:Audience"];
            var issuer = DBFunctionsMySQLBase.configSettings["Values:Issuer"];
            int expiryHrs = 8760;
            int.TryParse(DBFunctionsMySQLBase.configSettings["Values:ExpiryTimeInHrs"], out expiryHrs);
            int serviceExpiryHrs = 8760;
            int.TryParse(DBFunctionsMySQLBase.configSettings["Values:ServiceExpiryTimeInHrs"], out serviceExpiryHrs);
            //End

            // Register the access token provider as a singleton
            builder.Services.AddSingleton<IAccessTokenProvider, AccessTokenProvider>(s => new AccessTokenProvider(issuerToken, audience, issuer, expiryHrs, serviceExpiryHrs));
            //End Jwt Token

            builder.Services.Configure<TwilioSettings>(DBFunctionsMySQLBase.configSettings.GetSection(nameof(TwilioSettings)));

            string connectionString = SettingReader.ConfigSettings["ConnectionStrings:SqlConnectionString"];
            builder.Services.AddDbContextPool<ApplicationDBContext>(optionsBuilder=>
            {
                optionsBuilder.UseMySQL(connectionString);
            });

            builder.Services.Configure<CacheConnectionOptions>(SettingReader.ConfigSettings.GetSection("Redis"));
            builder.Services.AddSingleton(typeof(ICacheProvider), typeof(RedisCacheProvider));

            //builder.Services.AddTransient<IHomeDashboard, HomeUow>();
            builder.Services.AddTransient<ISecurity, LoginSecurity>();
            builder.Services.AddTransient<ISendEmail, SendEmail>();
            builder.Services.AddTransient<IDashboard, Dashboard>();
            builder.Services.AddTransient<IAboutUs, AboutUs>();
            builder.Services.AddTransient<IDoctorProfile, DoctorProfile>();
            builder.Services.AddTransient<ICommunity, Community>();
            builder.Services.AddTransient<IVideoCalling, VideoCalling>();
            builder.Services.AddTransient<IVideoService, VideoService>();
            builder.Services.AddTransient<IRecords, Record>();
            builder.Services.AddTransient<ISetting, Setting>();
            builder.Services.AddTransient<IHomeAnalytics, HomeScreenAnalytics>();
            builder.Services.AddTransient<IUserPermission, UserPermission>();
            builder.Services.AddTransient<IMessage, Message>();
            builder.Services.AddTransient<IQRScan, QRScan>();
            builder.Services.AddTransient<IVaccine, Vaccine>();
            builder.Services.AddTransient<ITreatMentAPI, TreatMentApi>();
            builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
        }
    }
}
