using System;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using System.Threading.Tasks;
using System.Linq;
using Amazon;

namespace AWSTests
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello Cloud Developers!!!");
            var c = new AWSSNSClient();
            var t = c.Go();
            Console.ReadKey();
        }
    }

    public class AWSCredentials
    {
        public string AccessKey {get; set;}
        public string SecretKey { get; set; }
        public RegionEndpoint Region { get; set; }

        public AWSCredentials(string accessKey, string secretKey, RegionEndpoint region)
        {
            AccessKey = accessKey;
            SecretKey = secretKey;
            Region = region;
        }
    }

    public class AWSSNSClient
    {
        public async Task SendAWSNotification(AWSCredentials credentials, string topicArn, string message)
        {
            Console.WriteLine();
            Console.WriteLine("--SENDING-MESSAGE----------------------");
            var client = new AmazonSimpleNotificationServiceClient(credentials.AccessKey, credentials.SecretKey, credentials.Region);
            Console.WriteLine("Pushing message to topic: {0}", topicArn);      
            var publishResponse = await client.PublishAsync(topicArn, message);
            Console.WriteLine("Message sent with HttpStatusCode: {0}", publishResponse.HttpStatusCode);
            Console.WriteLine("---------------------------------------");
            Console.WriteLine();
            Console.WriteLine("--SENDING-MESSAGE-USING-REQUEST--------");
            Console.WriteLine("Pushing message to topic: {0} using PublishRequest", topicArn);
            var publishRequest = new PublishRequest();
            publishRequest.Subject = "Message subject";
            publishRequest.Message = message;
            publishRequest.TopicArn = topicArn;
            publishResponse = await client.PublishAsync(publishRequest);
            Console.WriteLine("Message sent with HttpStatusCode: {0}", publishResponse.HttpStatusCode);
            Console.WriteLine("---------------------------------------");
            Console.WriteLine();
            Console.WriteLine("--SENDING-MESSAGE-USING-PHONE-REQUEST--");
            Console.WriteLine("Pushing message to topic: {0} using PublishRequest", topicArn);
            var phonePublishRequest = new PublishRequest();
            phonePublishRequest.Message = message;
            phonePublishRequest.PhoneNumber = "YOUR-PHONE-NUMBER";
            publishResponse = await client.PublishAsync(phonePublishRequest);
            Console.WriteLine("Message sent with HttpStatusCode: {0}", publishResponse.HttpStatusCode);
            Console.WriteLine("---------------------------------------");
        }

        public async Task GetAWSTopics(AWSCredentials credentials)
        {
            var client = new AmazonSimpleNotificationServiceClient(credentials.AccessKey, credentials.SecretKey, credentials.Region);
            var request = new ListTopicsRequest();
            var response = new ListTopicsResponse();
            do
            {
                response = await client.ListTopicsAsync(request);

                foreach (var topic in response.Topics)
                {
                    Console.WriteLine("Topic: {0}", topic.TopicArn);
                    await SendAWSNotification(credentials, topic.TopicArn, String.Format("Message from topic: {0}", topic.TopicArn));
                    var subs = await client.ListSubscriptionsByTopicAsync(
                      new ListSubscriptionsByTopicRequest
                      {
                          TopicArn = topic.TopicArn
                      });

                    var ss = subs.Subscriptions;

                    if (ss.Any())
                    {
                        Console.WriteLine("  Subscriptions:");
                        foreach (var sub in ss)
                        {
                            Console.WriteLine("    {0}", sub.SubscriptionArn);
                        }
                    }

                    var attrs = await client.GetTopicAttributesAsync(
                      new GetTopicAttributesRequest
                      {
                          TopicArn = topic.TopicArn
                      });

                    if (attrs.Attributes.Any())
                    {
                        Console.WriteLine("  Attributes:");

                        foreach (var attr in attrs.Attributes)
                        {
                            Console.WriteLine("    {0} = {1}", attr.Key, attr.Value);
                        }
                    }
                    Console.WriteLine();
                }
                request.NextToken = response.NextToken;
            } while (!string.IsNullOrEmpty(response.NextToken));
        }

        public async Task<int> Go()
        {
            try
            {
                var credentials = new AWSCredentials("ACCESS-KEY", "SECRET-ACCESS-KEY", RegionEndpoint.EUWest1);
                await GetAWSTopics(credentials); 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return 1;
        }
    }
}
