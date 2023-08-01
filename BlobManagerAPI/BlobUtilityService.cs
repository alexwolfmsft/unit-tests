using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace FileUpload
{
    public class BlobUtilityService
    {
        private BlobServiceClient _blobServiceClient;

        public BlobUtilityService(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        public async Task UploadAsBlob(IFormFile file)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient("products");

            var cleanName = file.FileName.ToLower();

            await containerClient.UploadBlobAsync(cleanName, file.OpenReadStream());
        }

        public async Task<List<BlobItem>> GetUntaggedBlobsAsync()
        {
            var container = _blobServiceClient.GetBlobContainerClient("products");

            var unTaggedBlobs = new List<BlobItem>();

            await foreach(var blob in container.GetBlobsAsync(BlobTraits.None, BlobStates.All, null, default))
            {
                if (blob.Tags == null || blob.Tags.Count == 0)
                {
                    unTaggedBlobs.Add(blob);
                }
            }

            return unTaggedBlobs;
        }
    }
}
