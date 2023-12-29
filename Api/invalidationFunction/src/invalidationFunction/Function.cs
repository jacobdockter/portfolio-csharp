using Amazon.Lambda.Core;
using Amazon.CloudFront;
using Amazon.CloudFront.Model;
using Amazon.CodePipeline;
using Amazon.CodePipeline.Model;
using System.Collections.Generic;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace invalidationFunction
{
    public class Function
    {
        public string FunctionHandler(ILambdaContext context)
        {
            try
            {
                context.Logger.LogLine("Beginning to invalidate CloudFront distribution...");

                // Get codepipeline job id from context
                string codepipelineJobId = context.AwsRequestId;

                // Get distribution id from user parameters
                string distributionId = System.Environment.GetEnvironmentVariable("DISTRIBUTION_ID");

                // Create CloudFront client
                var cloudfrontClient = new AmazonCloudFrontClient();

                // Create invalidation request
                var invalidationRequest = new CreateInvalidationRequest
                {
                    DistributionId = distributionId,
                    InvalidationBatch = new InvalidationBatch
                    {
                        CallerReference = codepipelineJobId,
                        Paths = new Paths
                        {
                            Quantity = 1,
                            Items = new List<string> { "/*" }
                        }
                    }
                };

                // Submit invalidation request
                var response = cloudfrontClient.CreateInvalidationAsync(invalidationRequest).Result;

                // Get CodePipeline client
                var codepipelineClient = new AmazonCodePipelineClient();

                // Mark CodePipeline job as success
                var successResultRequest = new PutJobSuccessResultRequest
                {
                    JobId = codepipelineJobId
                };
                codepipelineClient.PutJobSuccessResultAsync(successResultRequest).Wait();

                context.Logger.LogLine($"Invalidation request submitted successfully. Invalidation ID: {response.Invalidation.Id}");

                return response.Invalidation.Id;
            }
            catch (Exception ex)
            {
                // Get CodePipeline client
                var codepipelineClient = new AmazonCodePipelineClient();

                // Mark CodePipeline job as failed
                var failureResultRequest = new PutJobFailureResultRequest
                {
                    JobId = codepipelineJobId,
                    FailureDetails = new FailureDetails
                    {
                        Message = ex.Message,
                        Type = FailureType.JobFailed
                    }
                };
                codepipelineClient.PutJobFailureResultAsync(failureResultRequest).Wait();

                context.Logger.LogLine($"Invalidation failed. Error occurred: {ex.Message}");
                throw;
            }
        }
    }
}