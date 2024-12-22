using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using Challenge.LargeFileSort.Sorter;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ChunkSorterLambda;

public class ChunkSorterLambdaHandler
{
	public static async Task Function(ChunkSortInput input, ILambdaContext context)
	{
		var s3Client = new AmazonS3Client();

		var chunkPath = "/tmp/chunk";
		var getObjectResponse = await s3Client.GetObjectAsync(input.BucketName, input.ChunkFile);
		using (var fileStream = File.Create(chunkPath))
		{
			await getObjectResponse.ResponseStream.CopyToAsync(fileStream);
		}

		var sortedChunkPath = "/tmp/sorted_chunk";
		FileChunkSorter.SortChunk(chunkPath, sortedChunkPath, input.SortingAlgo);

		await s3Client.PutObjectAsync(new PutObjectRequest
		{
			BucketName = input.BucketName,
			Key = $"sorted_chunks/{Path.GetFileName(input.ChunkFile)}",
			FilePath = sortedChunkPath
		});
	}
}

public class ChunkSortInput
{
	public required string BucketName { get; set; }
	public required string ChunkFile { get; set; }
	public required string SortingAlgo { get; set; }
}

