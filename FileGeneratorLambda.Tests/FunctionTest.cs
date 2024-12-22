using Amazon.S3;
using Amazon.S3.Model;
using Moq;
using Xunit;
using Amazon.Lambda.TestUtilities;

namespace FileGeneratorLambda.Tests;

public class FunctionTest
{
	[Fact]
	public async Task TestFileGenerationAndUpload()
	{
		// Arrange
		var bucketName = "large-file-sort";
		var fileName = "input/input_1.txt";
		var targetSizeBytes = 1024 * 1024; // 1 MB for testing

		var mockS3Client = new Mock<IAmazonS3>();

		mockS3Client.Setup(client => client.InitiateMultipartUploadAsync(It.IsAny<InitiateMultipartUploadRequest>(), default))
			.ReturnsAsync(new InitiateMultipartUploadResponse { UploadId = "test-upload-id" });

		mockS3Client.Setup(client => client.UploadPartAsync(It.IsAny<UploadPartRequest>(), default))
			.ReturnsAsync(new UploadPartResponse { PartNumber = 1, ETag = "test-etag" });


		var input = new S3Input
		{
			BucketName = bucketName,
			FileName = fileName,
			Size = targetSizeBytes
		};

		var context = new TestLambdaContext();

		// Act
		await FileGeneratorLambdaHandler.Function(input, context);

		// Assert
		mockS3Client.Verify(client => client.InitiateMultipartUploadAsync(It.Is<InitiateMultipartUploadRequest>(req => req.BucketName == bucketName && req.Key == fileName), default), Times.Once);
		mockS3Client.Verify(client => client.UploadPartAsync(It.Is<UploadPartRequest>(req => req.PartSize <= 5 * 1024 * 1024), default), Times.AtLeastOnce);
	}
}
