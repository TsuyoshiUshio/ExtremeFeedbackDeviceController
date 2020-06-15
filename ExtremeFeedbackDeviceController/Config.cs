using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace ExtremeFeedbackDeviceController
{
    public class Config
    {
        public string ContainerUrl { get; set; }
        public int NumberOfLumps { get; set; }
        public string HueUserName { get; set; }
        public string HueBridgeIp { get; set; }

        public const string AppSettings = "appsettings.json";

        public string this[string propertyName] => (string)this.GetType().GetProperty(propertyName)?.GetValue(this, null);

        private bool Valid = true; 
        private void Validate()
        {
            StringBuilder builder = new StringBuilder();
            ValidateProperty(builder, nameof(ContainerUrl));
            ValidateProperty(builder, nameof(HueUserName));
            ValidateProperty(builder, nameof(HueBridgeIp));
            if (NumberOfLumps == 0)
            {
                builder.Append($"Error: {nameof(NumberOfLumps)} is Empty\n");
                Valid = false;
            }

            if (!Valid)
            {
                throw new ArgumentException($"Some of configuration missing. Consider to create {AppSettings} or set EnvironmentVariables. \n {builder.ToString()}");
            }
        }

        private void ValidateProperty(StringBuilder builder, string propertyName)
        {
            if (string.IsNullOrEmpty(this[propertyName]))
            {
                builder.Append($"Error: {propertyName} is Empty\n");
                Valid = false;
            }
        }

        public static Config Initialize()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();
            if (File.Exists(AppSettings))
            {
                builder.AddJsonFile(AppSettings);
            }

            builder.AddEnvironmentVariables();
            var configuration = builder.Build();
            var config = new Config()
            {
                ContainerUrl = configuration[nameof(ContainerUrl)],
                NumberOfLumps = int.Parse(configuration[nameof(NumberOfLumps)]),
                HueUserName = configuration[nameof(HueUserName)],
                HueBridgeIp = configuration[nameof(HueBridgeIp)]
            };

            config.Validate();
            return config;
        }
    }
}
