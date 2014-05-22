using System;
using System.Collections.Generic;
using System.Configuration;
using AgeBase.ExtendedDistributedCalling.Interfaces;
using Amazon;

namespace AgeBase.ExtendedDistributedCalling.Providers
{
    public abstract class AmazonDistributedCallingProvider : IExtendedDistributedCallingProvider
    {
        protected static string GetSecretKeyFromConfiguration()
        {
            var secretKey = ConfigurationManager.AppSettings["AWS_SECRET_KEY"];
            if (secretKey == null)
                throw new ArgumentException("Missing AWS_SECRET_KEY app setting");
            return secretKey;
        }

        protected static string GetAccessKeyFromConfiguration()
        {
            var accessKey = ConfigurationManager.AppSettings["AWS_ACCESS_KEY_ID"];
            if (accessKey == null)
                throw new ArgumentException("Missing AWS_ACCESS_KEY_ID app setting");
            return accessKey;
        }
        
        protected static RegionEndpoint GetAwsRegionFromConfiguration()
        {
            var awsRegionName = ConfigurationManager.AppSettings["AWS_REGION"];
            if (awsRegionName == null)
                throw new ArgumentException("Missing AWS_REGION app setting");

            switch (awsRegionName.Trim().ToLower())
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