using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3.Model;

namespace s3_file_manager_backend.Services
{
    public class S3Service
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public S3Service(IConfiguration configuration)
        {
            var awsAccessKeyId = configuration["AWS:AccessKey"];
            var awsSecretAccessKey = configuration["AWS:SecretKey"];
            var awsSessionToken = configuration["AWS:SessionToken"];
            var region = configuration["AWS:Region"];
            _bucketName = configuration["AWS:BucketName"];

            _s3Client = new AmazonS3Client(awsAccessKeyId, awsSecretAccessKey, awsSessionToken, RegionEndpoint.GetBySystemName(region));
        }

        public async Task UploadFileAsync(IFormFile file, string keyName)
        {
            using (var newMemoryStream = new MemoryStream())
            {
                file.CopyTo(newMemoryStream);
                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = newMemoryStream,
                    Key = keyName,
                    BucketName = _bucketName,
                    ContentType = file.ContentType
                };

                var fileTransferUtility = new TransferUtility(_s3Client);
                await fileTransferUtility.UploadAsync(uploadRequest);
            }
        }

        public async Task DeleteFileAsync(string keyName)
        {
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = keyName
            };

            await _s3Client.DeleteObjectAsync(deleteRequest);
        }

        public string GenerateFileLink(string keyName)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = keyName,
                Expires = DateTime.UtcNow.AddYears(10)
            };

            request.ResponseHeaderOverrides.ContentType = "application/force-download";
            string url = _s3Client.GetPreSignedURL(request);
            return url;
        }
    }

}
