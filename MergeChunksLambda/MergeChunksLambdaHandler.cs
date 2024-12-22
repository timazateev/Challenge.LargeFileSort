using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using Challenge.LargeFileSort.Splitter;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace MergeChunksLambda;

public class MergeChunksLambdaHandler
{
	public static async Task Function(MergeInput input, ILambdaContext context)
	{
		var s3Client = new AmazonS3Client();

		var sortedChunkPaths = new List<string>();
		foreach (var chunkFile in input.SortedChunks)
		{
			var localPath = $"/tmp/{Path.GetFileName(chunkFile)}";
			var getObjectResponse = await s3Client.GetObjectAsync(input.BucketName, chunkFile);
			using (var fileStream = File.Create(localPath))
			{
				await getObjectResponse.ResponseStream.CopyToAsync(fileStream);
			}
			sortedChunkPaths.Add(localPath);
		}

		var outputFilePath = "/tmp/final_output";
		FileSorterHelpers.MergeSortedChunks(sortedChunkPaths, outputFilePath);

		await s3Client.PutObjectAsync(new PutObjectRequest
		{
			BucketName = input.BucketName,
			Key = input.OutputFile,
			FilePath = outputFilePath
		});
	}
}

public class MergeInput
{
	public required string BucketName { get; set; }
	public required string OutputFile { get; set; }
	public required List<string> SortedChunks { get; set; }
}

