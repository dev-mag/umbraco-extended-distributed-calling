using System.Collections.Generic;
using System.Linq;
using Amazon.EC2;
using Amazon.EC2.Model;
using Amazon.ElasticBeanstalk;
using Amazon.ElasticBeanstalk.Model;
using Amazon.ElasticLoadBalancing;
using Amazon.ElasticLoadBalancing.Model;

namespace AgeBase.ExtendedDistributedCalling.Providers
{
    public class BeanstalkDistributedCallingProvider : AmazonDistributedCallingProvider
    {
        public override List<string> GetServers()
        {
            var accessKey = GetAccessKeyFromConfiguration();
            var secretKey = GetSecretKeyFromConfiguration();
            var environmentName = GetEnvironmentNameFromConfiguration();
            var regionEndpoint = GetAwsRegionFromConfiguration();

            // Create client
            var elasticBeanstalkClient = new AmazonElasticBeanstalkClient(accessKey, secretKey, regionEndpoint);

            // Get environment resources for environment
            var environmentResourcesRequest = new DescribeEnvironmentResourcesRequest { EnvironmentName = environmentName };
            var resourceResponse = elasticBeanstalkClient.DescribeEnvironmentResources(environmentResourcesRequest);

            // Create ELB client
            var elasticLoadBalancingClient = new AmazonElasticLoadBalancingClient(accessKey, secretKey, regionEndpoint);

            // Get load balancers for all environment's load balancers
            var loadBalancersRequest = new DescribeLoadBalancersRequest();

            foreach (var loadBalancer in resourceResponse.EnvironmentResources.LoadBalancers)
                loadBalancersRequest.LoadBalancerNames.Add(loadBalancer.Name);

            var describeLoadBalancersResponse = elasticLoadBalancingClient.DescribeLoadBalancers(loadBalancersRequest);

            // Create EC2 client
            var ec2Client = new AmazonEC2Client(accessKey, secretKey, regionEndpoint);

            // Get instances for all instance ids in all load balancers
            var instancesRequest = new DescribeInstancesRequest();

            // Get all instance ids for all load balancers
            foreach (var instance in describeLoadBalancersResponse.LoadBalancerDescriptions.SelectMany(loadBalancer => loadBalancer.Instances))
                instancesRequest.InstanceIds.Add(instance.InstanceId);

            var instancesResponse = ec2Client.DescribeInstances(instancesRequest);

            // Find all private dns names
            return (from reservation in instancesResponse.Reservations from instance in reservation.Instances select instance.PrivateDnsName).ToList();
        }


    }
}