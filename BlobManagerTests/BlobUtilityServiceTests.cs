using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FileUpload;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;

namespace FileUploadTests
{
    public class BlobUtilityServiceTests
    {
        [Fact]
        public async Task UploadBlob_ShouldHaveCleanName()
        {
            // Arrange
            var blobServiceMock = new Mock<BlobServiceClient>();
            var containerMock = new Mock<BlobContainerClient>();

            blobServiceMock.Setup(x => x.GetBlobContainerClient("products")).Returns(containerMock.Object);
            containerMock.Setup(x => x.UploadBlobAsync(It.IsAny<string>(), It.IsAny<Stream>(), default))
                .ReturnsAsync(Mock.Of<Response<BlobContentInfo>>());

            using var stream = File.OpenRead("SAMPLE.txt");
            var file = new FormFile(stream, 0, stream.Length, stream.Name, Path.GetFileName(stream.Name))
            {
                Headers = new HeaderDictionary(),
                ContentType = "text"
            };

            var fileHelperService = new BlobUtilityService(blobServiceMock.Object);

            // Act
            await fileHelperService.UploadAsBlob(file);

            // Assert
            containerMock.Verify(x => x.UploadBlobAsync("sample.txt", It.IsAny<Stream>(), default), Times.Once);
        }


        [Fact]
        public async Task GetUntaggedBlobs_ShouldNotReturnTaggedBlobs()
        {
            // Arrange
            Page<BlobItem> page = Page<BlobItem>.FromValues(new List<BlobItem>
            {
                BlobsModelFactory.BlobItem(name: "Hello", tags: new Dictionary<string, string>(){ { "category", "book" } }),
                BlobsModelFactory.BlobItem(name: "World", tags: new Dictionary<string, string>())
            },
            null, Mock.Of<Response>());

            AsyncPageable<BlobItem> pageable = AsyncPageable<BlobItem>.FromPages(new[] { page });

            var blobServiceMock = new Mock<BlobServiceClient>();
            var containerMock = new Mock<BlobContainerClient>();

            containerMock.Setup(x => x.GetBlobsAsync(BlobTraits.None, BlobStates.All, null, It.IsAny<CancellationToken>())).Returns(pageable);
            blobServiceMock.Setup(x => x.GetBlobContainerClient("products")).Returns(containerMock.Object);

            var uploadService = new BlobUtilityService(blobServiceMock.Object);

            // Act
            var untaggedBlobs = await uploadService.GetUntaggedBlobsAsync();

            // Assert
            Assert.True(untaggedBlobs.Count == 1);
            Assert.True(untaggedBlobs[0].Name == "World");
        }
    }
}