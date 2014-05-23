using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Amazon.EC2;
using Amazon.EC2.Model;
using Amazon.EC2.Util;
using log4net;

namespace AgeBase.ExtendedDistributedCalling.Providers
{
    public class TaggedServerDistributedCallingProvider : AmazonDistributedCallingProvider
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (TaggedServerDistributedCallingProvider));
          
        protected static string GetServerTagNameFromConfiguration()
        {
            var environmentName = ConfigurationManager.AppSettings["AWS_INSTANCE_TAG_NAME"];
            if (environmentName == null)
                throw new ArgumentException("Missing AWS_INSTANCE_TAG_NAME app setting");
            return environmentName;
        }

        public override List<string> GetServers()
        {
            var accessKey = GetAccessKeyFromConfiguration();
            var secretKey = GetSecretKeyFromConfiguration();
            var tagName = GetServerTagNameFromConfiguration();
            var regionEndpoint = GetAwsRegionFromConfiguration();

            Log.DebugFormat("Connecting to EC2 API service in region `{0}`", regionEndpoint);

            var client = new AmazonEC2Client(
                accessKey, 
                secretKey,
                regionEndpoint);

            var myInstanceId = EC2Metadata.InstanceId;

            var tagValue = GetInstanceTagValue(client, myInstanceId, tagName);
            var instances = FindInstancesWithTags(client, tagName, tagValue);

            return instances
                .Select(x => x.PrivateIpAddress)
                .ToList();
        }

        private static IEnumerable<Instance> FindInstancesWithTags(
            IAmazonEC2 client,
            string tagName, 
            string tagValue)
        {
            var request = new DescribeInstancesRequest
            {
                Filters = new List<Filter>
                {
                    new Filter { Name = "tag:" + tagName, Values = new List<string> { tagValue } }
                }
            };

            var response = client.DescribeInstances(request);

            var instances = response
                .Reservations
                .SelectMany(x => x.Instances)
                .ToList();

            Log.DebugFormat("Found {0} instances", instances.Count);

            return instances;
        }

        private static string GetInstanceTagValue(IAmazonEC2 client, string instanceId, string tagName)
        {
            Log.DebugFormat("Retrieving tag `{0}` for current server instance `{1}`", tagName, instanceId);

            var instance = FindInstance(client, instanceId);
            var tag = GetTagValue(tagName, instance);

            Log.DebugFormat("Tag found. `{0}` = `{1}`", tagName, tag.Value);

            return tag.Value;
        }

        private static Instance FindInstance(IAmazonEC2 client, string instanceId)
        {
            var request = new DescribeInstancesRequest
            {
                InstanceIds = new List<string> {instanceId}
            };

            var describeResponse = client.DescribeInstances(request);

            var instance = describeResponse.Reservations.SelectMany(x => x.Instances).SingleOrDefault();
            if (instance != null)
                return instance;

            var error = string.Format("Could not find instance with id `{0}`", instanceId);
            Log.Error(error);
            throw new ArgumentException(error, "instanceId");
        }

        private static Tag GetTagValue(string tagName, Instance instance)
        {
            var tag = instance.Tags.SingleOrDefault(x => x.Key.Equals(tagName, StringComparison.InvariantCultureIgnoreCase));
            if (tag != null)
                return tag;

            var error = string.Format("Could not find tag `{0}` on instance {1}", tagName, instance.InstanceId);
            Log.Error(error);
            throw new ArgumentException(error, "tagName");
        }
    }
}
