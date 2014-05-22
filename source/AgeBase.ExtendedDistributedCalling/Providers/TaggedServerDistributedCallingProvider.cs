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
            if (ConfigurationManager.AppSettings["SERVER_TAG_NAME"] == null)
                throw new ArgumentException("Missing SERVER_TAG_NAME app setting");
            var environmentName = ConfigurationManager.AppSettings["SERVER_TAG_NAME"];
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
            var instances = FindInstancesWithTags(client, tagName, tagValue, myInstanceId);

            return instances
                .Select(x => x.InstanceId)
                .ToList();
        }

        private static IEnumerable<Instance> FindInstancesWithTags(
            IAmazonEC2 client,
            string tagName, 
            string tagValue, 
            string myInstanceId)
        {
            var request = new DescribeInstancesRequest
            {
                Filters = new List<Filter>
                {
                    new Filter { Name = tagName, Values = new List<string> { tagValue } }
                }
            };

            var response = client.DescribeInstances(request);

            var instances = response
                .Reservations
                .SelectMany(x => x.Instances)
                .Where(x => !x.InstanceId.Equals(myInstanceId, StringComparison.InvariantCultureIgnoreCase))
                .ToList();

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
            var instance = describeResponse.Reservations.SelectMany(x => x.Instances).Single();
            return instance;
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
