using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using System.Text;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace FileGeneratorLambda;

public class FileGeneratorLambdaHandler
{
	public static async Task Function(S3Input input, ILambdaContext context)
	{
		var s3Client = new AmazonS3Client();

		string bucketName = input.BucketName;
		string key = input.FileName;
		long targetSizeBytes = input.Size;

		context.Logger.LogLine($"Starting file generation for bucket: {bucketName}, key: {key}, size: {targetSizeBytes} bytes");

		try
		{
			using var stream = new MemoryStream();
			using var writer = new StreamWriter(stream, Encoding.UTF8, 65536, leaveOpen: true);

			var rnd = new Random();
			long currentSize = 0;

			while (currentSize < targetSizeBytes)
			{
				string line = $"RandomNumber_{rnd.Next(1, 1_000_000)}\n";
				byte[] lineBytes = Encoding.UTF8.GetBytes(line);

				if (currentSize + lineBytes.Length > targetSizeBytes)
					break;

				writer.Write(line);
				currentSize += lineBytes.Length;
			}

			writer.Flush();
			stream.Seek(0, SeekOrigin.Begin);

			context.Logger.LogLine($"Uploading file to S3: Bucket = {bucketName}, Key = {key}, Size = {stream.Length} bytes");

			var uploadRequest = new PutObjectRequest
			{
				BucketName = bucketName,
				Key = key,
				InputStream = stream
			};

			await s3Client.PutObjectAsync(uploadRequest);

			context.Logger.LogLine("Upload completed successfully.");
		}
		catch (Exception ex)
		{
			context.Logger.LogLine($"Error during file upload: {ex.Message}");
			throw;
		}
	}
}

public class S3Input
{
	public required string BucketName { get; set; }
	public required string FileName { get; set; }
	public required long Size { get; set; }
}
