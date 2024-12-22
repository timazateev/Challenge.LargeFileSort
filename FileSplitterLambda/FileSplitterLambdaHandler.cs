using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using Challenge.LargeFileSort.Splitter;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace FileSplitterLambda;

public class FileSplitterLambdaHandler
{
	public async Task Function(S3Input input, ILambdaContext context)
	{
		var s3Client = new AmazonS3Client();

		// Скачиваем файл из S3
		var filePath = "/tmp/input_file";
		var getObjectResponse = await s3Client.GetObjectAsync(input.BucketName, input.FileName);
		using (var fileStream = File.Create(filePath))
		{
			await getObjectResponse.ResponseStream.CopyToAsync(fileStream);
		}

		// Разбиваем файл на чанки
		var chunkFiles = FileSorterHelpers.SplitIntoLineAlignedChunks(filePath, input.ChunkSize);

		// Загружаем чанки в S3
		foreach (var chunkFile in chunkFiles)
		{
			var chunkFileName = Path.GetFileName(chunkFile);
			await s3Client.PutObjectAsync(new PutObjectRequest
			{
				BucketName = input.BucketName,
				Key = $"chunks/{chunkFileName}",
				FilePath = chunkFile
			});
		}
	}

	public class S3Input
	{
		public required string BucketName { get; set; }
		public required string FileName { get; set; }
		public required long Size { get; set; }
	}
}

