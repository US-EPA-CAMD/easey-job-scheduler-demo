using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Quartz;
using SilkierQuartz;

namespace Epa.Camd.Easey.JobScheduler.Jobs
{
    public class HelloJob : IJob
    {
        private string connectionString;
        private IConfiguration Configuration { get; }
        private static IAmazonS3 s3Client;
        private const string bucketName = "cg-e7433599-fb63-4afa-9e14-652c10fa370a";
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.USGovCloudWest1;
        private string baseUri = "https://easey-dev.app.cloud.gov/api/emissions-mgmt/apportioned/hourly?";
        private string baseUri2 = "https://easey-dev.app.cloud.gov/api/monitor-plan-mgmt/monitor-locations";

        public async Task Execute(IJobExecutionContext context)
        
        {

            JobDataMap dataMap = context.Trigger.JobDataMap;

            foreach (var item in dataMap)
            {
                baseUri2 += $"/{item.Value}&";
            }

            baseUri2 = baseUri2.Remove(baseUri2.Length - 1, 1);

            await Console.Out.WriteLineAsync("Retrieving data from api...");
            baseUri2 = baseUri2 + "/components";


            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(baseUri2);
            request.ContentType = "application/json; charset=utf-8";
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            using (Stream responseStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                Console.WriteLine(reader.ReadToEnd());
            }


            //HttpClient httpClient = new HttpClient();
            //var results = await httpClient.GetStringAsync(baseUri2 + "/components");

            //await Console.Out.WriteLineAsync("Wrting results to S3....");
            //s3Client = new AmazonS3Client("AKIAR7FXZINYN4HBHHMA", "8M1iRpD2X7Of2s37VaJNHQ/4uifoBcM52B91KWlP", bucketRegion);
            //var s3Request = new PutObjectRequest
            //{
            //    BucketName = bucketName,
            //    Key = context.JobDetail.Key.Name,
            //    ContentBody = results
            //};

            //PutObjectResponse s3Response = await s3Client.PutObjectAsync(s3Request);


            //await Console.Out.WriteLineAsync(results);
        }
    }
}
