using System;
using System.Net.Http;
using System.Threading.Tasks;

using Quartz;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

namespace EPA.CAMPD.Scheduler
{
  public class SubJob : IJob
  {
    private static IAmazonS3 s3Client;
    private const string bucketName = "cg-e7433599-fb63-4afa-9e14-652c10fa370a";
    private static readonly RegionEndpoint bucketRegion = RegionEndpoint.USGovCloudWest1;
    private string baseUri = "https://easey-dev.app.cloud.gov/api/emissions-mgmt/apportioned/hourly?";

    public async Task Execute(IJobExecutionContext context)
    {
      JobDataMap dataMap = context.JobDetail.JobDataMap;

      foreach(var item in dataMap)
      {
        baseUri += $"{item.Key}={item.Value}&";
      }

      baseUri = baseUri.Remove(baseUri.Length-1, 1);

      await Console.Out.WriteLineAsync("Retrieving data from api...");

      HttpClient httpClient = new HttpClient();
      var results = await httpClient.GetStringAsync(baseUri);

      await Console.Out.WriteLineAsync("Wrting results to S3....");
      s3Client = new AmazonS3Client("<AWS_ACCESS_KEY>", "<AWS_SECRET_KEY>", bucketRegion);
      var s3Request = new PutObjectRequest
      {
        BucketName = bucketName,
        Key = context.JobDetail.Key.Name,
        ContentBody = results
      };

      PutObjectResponse s3Response = await s3Client.PutObjectAsync(s3Request);

      await Console.Out.WriteLineAsync("Done....");
    }
  }
}