using System;
using System.Data;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace UneecopsTechnologies.DronaDoctorApp.API
{
    public static class SettingReader
    {
        private static IConfigurationRoot _configSettings;

        public static IConfigurationRoot ConfigSettings
        {
            get
            {
                return _configSettings;
            }
        }

        static SettingReader()
        {
           //This would be removed once Release Configuration is in place

            string isLocal = Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID");
            string env = Environment.GetEnvironmentVariable("WEBSITE_DEPLOYMENT_ID");
            var currentDirectory = "/home/site/wwwroot";
            
            //WEBSITE_DEPLOYMENT_ID should be specific to env to use
            
            if (string.IsNullOrEmpty(isLocal) || string.IsNullOrEmpty(env))
            {
                currentDirectory = Environment.CurrentDirectory;
                env = "development";
            }
            string setting = $"{env}.settings.json";
            _configSettings = new ConfigurationBuilder().SetBasePath(currentDirectory).
                                            AddJsonFile(setting, optional: true, reloadOnChange: true).
                                            AddEnvironmentVariables().Build();
        }
    }
}
