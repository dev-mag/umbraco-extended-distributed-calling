using System;
using System.Collections.Generic;
using System.Configuration;
using AgeBase.ExtendedDistributedCalling.Interfaces;
using Amazon;

namespace AgeBase.ExtendedDistributedCalling.Providers
{
    public abstract class AmazonDistributedCallingProvider : IExtendedDistributedCallingProvider
    {
        protected static string GetEnvironmentNameFromConfiguration()
        {
            if (ConfigurationManager.AppSettings["AWS_ENV_NAME"] == null)
                throw new ArgumentException("Missing AWS_ENV_NAME app setting");
            var environmentName = ConfigurationManager.AppSettings["AWS_ENV_NAME"];
            return environmentName;
        }

        protected static string GetSecretKeyFromConfiguration()
        {
            if (ConfigurationManager.AppSettings["AWS_SECRET_KEY"] == null)
                throw new ArgumentException("Missing AWS_SECRET_KEY app setting");
            var secretKey = ConfigurationManager.AppSettings["AWS_SECRET_KEY"];
            return secretKey;
        }

        protected static string GetAccessKeyFromConfiguration()
        {
            if (ConfigurationManager.AppSettings["AWS_ACCESS_KEY_ID"] == null)
                throw new ArgumentException("Missing AWS_ACCESS_KEY_ID app setting");
            var accessKey = ConfigurationManager.AppSettings["AWS_ACCESS_KEY_ID"];
            return accessKey;
        }
        
        protected static RegionEndpoint GetAwsRegionFromConfiguration()
        {
            if (ConfigurationManager.AppSettings["AWS_REGION"] == null)
                throw new ArgumentException("Missing AWS_REGION app setting");

            switch (ConfigurationManager.AppSettings["AWS_REGION"].Trim().ToLower())
            {
                case "us-east-1":
                    return RegionEndpoint.USEast1;
                case "us-west-1":
                    return RegionEndpoint.USWest1;
                case "us-west-2":
                    return RegionEndpoint.USWest2;
                case "eu-west-1":
                    return RegionEndpoint.EUWest1;
                case "ap-northeast-1":
                    return RegionEndpoint.APNortheast1;
                case "ap-southeast-1":
                    return RegionEndpoint.APSoutheast1;
                case "ap-southeast-2":
                    return RegionEndpoint.APSoutheast2;
                case "sa-east-1":
                    return RegionEndpoint.SAEast1;
                case "us-gov-west-1":
                    return RegionEndpoint.USGovCloudWest1;
                case "cn-north-1":
                    return RegionEndpoint.CNNorth1;
                default:
                    throw new ArgumentException("Incorrect AWS_REGION endpoint");
            }
        }

        public abstract List<string> GetServers();
    }
}