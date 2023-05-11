using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Cosmos.BlobService.Config;
using Cosmos.BlobService.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.BlobService.Drivers
{
    /// <summary>
    ///     AWS S3 storage driver
    /// </summary>
    public sealed class AmazonStorage : ICosmosStorage
    {

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="config"></param>
        /// <param name="distributedCache"></param>
        /// <remarks>
        ///     <para>
        ///         At present, AWS S3 doesn't have the ability to track byte chunks uploaded to a blob like Azure.
        ///         When all the chunks are uploaded, the developer needs to let AWS know all the chunks that need to be
        ///         combined. AWS is designed for the client to track the chunks.  The client needs to track this.
        ///         Problem is, the client in this case is a web browser, that uploads to the web server the
        ///         chunks.  The web browser using Telerik controls can't track this. So, we track the chunks
        ///         by storying the state in cache.  That is why <see cref="IMemoryCache" /> is needed.
        ///     </para>
        /// </remarks>
        public AmazonStorage(AmazonStorageConfig config, IMemoryCache cache)
        {
            _config = config;
            _cache = cache;
        }

        /// <summary>
        ///     Appends byte array to blob
        /// </summary>
        /// <param name="data"></param>
        /// <param name="fileMetaData"></param>
        /// <param name="uploadDateTime"></param>
        /// <returns></returns>
        public async Task AppendBlobAsync(byte[] data, FileUploadMetaData fileMetaData, DateTimeOffset uploadDateTime)
        {
            // ReSharper disable once PossibleNullReferenceException

            using var client = GetClient();

            if (fileMetaData.TotalChunks == 1)
            {
                // This is NOT a multi part upload
                await DeleteIfExistsAsync(fileMetaData.RelativePath);

                await using var memoryStream = new MemoryStream(data);
                // 2. Put the object-set ContentType and add metadata.
                var putRequest = new PutObjectRequest
                {
                    BucketName = _config.AmazonBucketName,
                    Key = fileMetaData.RelativePath,
                    InputStream = memoryStream,
                    ContentType = Utilities.GetContentType(fileMetaData)
                };

                putRequest.Metadata.Add("ccmsuploaduid", fileMetaData.UploadUid);
                putRequest.Metadata.Add("ccmssize", fileMetaData.TotalFileSize.ToString());
                putRequest.Metadata.Add("ccmsdatetime", uploadDateTime.UtcDateTime.Ticks.ToString());

                await client.PutObjectAsync(putRequest);

                // Dispose of the client.
                client.Dispose();

            }
            else
            {
                if (fileMetaData.ChunkIndex == 0)
                {
                    await DeleteIfExistsAsync(fileMetaData.RelativePath);

                    var initiateRequest = new InitiateMultipartUploadRequest
                    {
                        BucketName = _config.AmazonBucketName,
                        Key = fileMetaData.RelativePath,
                        Headers = { ContentType = Utilities.GetContentType(fileMetaData) }
                    };

                    initiateRequest.Metadata.Add("ccmsuploaduid", fileMetaData.UploadUid);
                    initiateRequest.Metadata.Add("ccmssize", fileMetaData.TotalFileSize.ToString());
                    initiateRequest.Metadata.Add("ccmsdatetime", uploadDateTime.UtcDateTime.Ticks.ToString());

                    var initiateUpload =
                        await client.InitiateMultipartUploadAsync(initiateRequest);

                    _cache.Set(
                        fileMetaData.UploadUid,
                        initiateUpload.UploadId,
                        new MemoryCacheEntryOptions
                        {
                            SlidingExpiration = TimeSpan.FromMinutes(15)
                        });

                    // Save the upload ID in the blob metadata
                    //await client.PutObjectAsync(request);
                }

                // This is the upload ID used to keep track of all the parts.
                var uploadId = (string)_cache.Get(fileMetaData.UploadUid);

                try
                {
                    var bytes = (byte[])_cache.Get(fileMetaData.UploadUid + "responses");

                    var responses = bytes == null
                        ? new List<UploadPartResponse>()
                        : JsonConvert.DeserializeObject<List<UploadPartResponse>>(Encoding.UTF32.GetString(bytes));


                    await using Stream stream = new MemoryStream(data);
                    stream.Position = 0;

                    // Upload 5 MB chunk here
                    var uploadRequest = new UploadPartRequest
                    {
                        BucketName = _config.AmazonBucketName,
                        Key = fileMetaData.RelativePath,
                        UploadId = uploadId, // get this from initiation
                        PartNumber = Convert.ToInt32(fileMetaData.ChunkIndex) + 1, // not 0 index based
                        PartSize = stream.Length,
                        InputStream = stream
                    };
                    var uploadResult = await client.UploadPartAsync(uploadRequest);
                    responses.Add(uploadResult);

                    if (fileMetaData.TotalChunks - 1 == fileMetaData.ChunkIndex)
                    {
                        // This is the last chunk, wrap things up here.

                        // Setup to complete the upload.
                        var completeRequest = new CompleteMultipartUploadRequest
                        {
                            BucketName = _config.AmazonBucketName,
                            Key = fileMetaData.RelativePath,
                            UploadId = uploadId
                        };

                        completeRequest.AddPartETags(responses);

                        // Complete the upload.
                        await client.CompleteMultipartUploadAsync(completeRequest);

                        _cache.Remove(fileMetaData.UploadUid + "responses");

                        // Dispose of the client.
                        client.Dispose();
                    }
                    else
                    {
                        // Cache the list for the next chunk upload
                        _cache.Set(fileMetaData.UploadUid + "responses",
                            Encoding.UTF32.GetBytes(JsonConvert.SerializeObject(responses)));
                    }
                }
                catch (Exception)
                {
                    // Something didn't go right, so cancel the multi part upload here.
                    var abortRequest = new AbortMultipartUploadRequest
                    {
                        BucketName = _config.AmazonBucketName,
                        Key = fileMetaData.RelativePath,
                        UploadId = uploadId
                    };
                    await client.AbortMultipartUploadAsync(abortRequest);
                    throw;
                }
            }
        }

        /// <summary>
        ///     Determines if a blob exists.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<bool> BlobExistsAsync(string path)
        {
            var blob = await GetBlobAsync(path);

            return blob != null && blob.HttpStatusCode != HttpStatusCode.NotFound;
        }

        /// <summary>
        ///     Copies a single blob and returns it's <see cref="CopyObjectResponse" />.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <returns>copy response</returns>
        /// <remarks>
        ///     Tip: After operation, check the returned blob object to see if it exists.
        /// </remarks>
        public async Task CopyBlobAsync(string source, string destination)
        {
            var sourceObject = await GetBlobAsync(source);

            if (sourceObject != null)
            {
                using var client = GetClient();

                await client.CopyObjectAsync(
                    _config.AmazonBucketName,
                    source,
                    _config.AmazonBucketName, destination);
            }
        }

        /// <summary>
        ///     Creates a folder if it does not yet exists.
        /// </summary>
        /// <param name="path">Full path to folder to create</param>
        /// <returns></returns>
        /// <exception cref="Exception">Folder creation failure.</exception>
        public async Task CreateFolderAsync(string path)
        {
            //
            // Blob storage does not have a folder object, just blobs with paths.
            // Therefore, to create an illusion of a folder, we have to create a blob
            // that will be in the folder.  For example:
            // 
            // To create folder /pictures 
            //
            // You have to pub a blob here /pictures/folder.subxx
            //
            // To remove a folder, you simply remove all blobs below /pictures
            //

            // Make sure the path doesn't already exist.
            // Don't lead with "/" with S3
            var fullPath = Utilities.GetBlobName(path, "folder.stubxx").TrimStart('/');

            var blob = await GetBlobAsync(fullPath);

            // Create or return existing here.
            if (blob == null)
            {
                var request = new PutObjectRequest
                {
                    BucketName = _config.AmazonBucketName,
                    Key = fullPath,
                    ContentBody = $"This is a folder stub file for {path}."
                };

                using var client = GetClient();

                await client.PutObjectAsync(request);
            }
        }

        /// <summary>
        ///     Gets a list of blobs by path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public async Task<List<string>> GetBlobNamesByPath(string path, string[] filter = null)
        {
            var blobList = await GetObjectsAsync(path, filter);
            return blobList.Select(s => s.Key).ToList();
        }

        /// <summary>
        ///     Delete a blob if it exists
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public async Task DeleteIfExistsAsync(string target)
        {
            try
            {
                var deleteObjectRequest = new DeleteObjectRequest
                {
                    BucketName = _config.AmazonBucketName,
                    Key = target
                };

                using var client = GetClient();

                await client.DeleteObjectAsync(deleteObjectRequest);
            }
            catch (AmazonS3Exception e)
            {
                if (e.StatusCode != HttpStatusCode.NotFound) throw;
            }
        }

        /// <summary>
        ///     Deletes all objects in a virtual folder.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public async Task<int> DeleteFolderAsync(string target)
        {
            var blobs = await GetObjectsAsync(target, null);

            if (blobs.Any())
            {
                using var client = GetClient();

                var deleteObjectsRequest = new DeleteObjectsRequest
                {
                    BucketName = _config.AmazonBucketName
                };

                foreach (var blob in blobs) deleteObjectsRequest.AddKey(blob.Key);

                var result = await client.DeleteObjectsAsync(deleteObjectsRequest);
                return result.DeletedObjects.Count;
            }

            return 0;
        }

        /// <summary>
        ///     Gets files and subfolders for a given path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="filters"></param>
        /// <returns></returns>
        public async Task<List<S3Object>> GetObjectsAsync(string path, string[] filters)
        {
            path = path.TrimStart('/');

            var request = new ListObjectsV2Request
            {
                BucketName = _config.AmazonBucketName
            };

            if (!string.IsNullOrEmpty(path)) request.Prefix = path;

            ListObjectsV2Response response;
            var blobList = new List<S3Object>();

            using var client = GetClient();

            do
            {
                response = await client.ListObjectsV2Async(request);

                // Process the response.
                foreach (var blobItem in response.S3Objects)
                {
                    //if (!blobItem.Key.StartsWith(folderName)) continue;
                    var extension = Path.GetExtension(blobItem.Key);
                    if (filters != null && !string.IsNullOrEmpty(extension) && filters.Contains(extension.ToLower()))
                        blobList.Add(blobItem);
                    else
                        blobList.Add(blobItem);
                }

                request.ContinuationToken = response.NextContinuationToken;
            } while (response.IsTruncated);

            return blobList;
        }

        /// <summary>
        ///     Gets a blob object
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public async Task<GetObjectResponse> GetBlobAsync(string target)
        {
            try
            {
                if (string.IsNullOrEmpty(target)) return null;

                target = target.TrimStart('/');

                using var client = GetClient();

                var response = await client.GetObjectAsync(_config.AmazonBucketName, target);
                return response;
            }
            catch (AmazonS3Exception e)
            {
                // object not found
                if (e.StatusCode == HttpStatusCode.NotFound)
                    return null;
                throw;
            }
        }

        /// <summary>
        /// Gets metadata for a blob
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public async Task<FileMetadata> GetFileMetadataAsync(string target)
        {
            var blob = await GetBlobAsync(target);

            _ = long.TryParse(blob.Metadata["ccmsuploaduid"], out var mark);
            return new FileMetadata()
            {
                ContentLength = blob.ContentLength,
                ContentType = blob.Headers.ContentType,
                ETag = blob.ETag,
                LastModified = DateTime.SpecifyKind(blob.LastModified, DateTimeKind.Utc)
            };
        }

        /// <summary>
        /// Gets an inventory of all blobs in a storage account
        /// </summary>
        /// <returns></returns>
        public async Task<List<FileMetadata>> GetInventory()
        {
            var blobs = await this.GetObjectsAsync("", null);

            return blobs.Select(s => new FileMetadata()
            {
                ContentLength = s.Size,
                ETag = s.ETag,
                FileName = s.ETag,
                LastModified = s.LastModified.ToUniversalTime()
            }).OrderBy(o => o.FileName).ToList();
        }

        /// <summary>
        /// Opens a read stream to download the blob.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public async Task<Stream> GetStreamAsync(string target)
        {
            var blob = await GetBlobAsync(target);
            return blob.ResponseStream;
        }

        #region PRIVATE FIELDS AND METHODS

        private readonly AmazonStorageConfig _config;
        private readonly IMemoryCache _cache;

        private AmazonS3Client GetClient()
        {
            var regionIdentifier = RegionEndpoint.GetBySystemName(_config.AmazonRegion);
            return new AmazonS3Client(_config.AmazonAwsAccessKeyId, _config.AmazonAwsSecretAccessKey, regionIdentifier);
        }

        /// <summary>
        /// Uploads file as a stream
        /// </summary>
        /// <param name="readStream"></param>
        /// <param name="fileMetaData"></param>
        /// <param name="uploadDateTime"></param>
        /// <returns></returns>
        public async Task<bool> UploadStreamAsync(Stream readStream, FileUploadMetaData fileMetaData, DateTimeOffset uploadDateTime)
        {
            using var client = GetClient();

            // Get rid of old blob
            await DeleteIfExistsAsync(fileMetaData.RelativePath);

            var putRequest = new PutObjectRequest
            {
                BucketName = _config.AmazonBucketName,
                Key = fileMetaData.RelativePath,
                InputStream = readStream,
                ContentType = Utilities.GetContentType(fileMetaData)
            };

            putRequest.Metadata.Add("ccmsuploaduid", fileMetaData.UploadUid);
            putRequest.Metadata.Add("ccmssize", fileMetaData.TotalFileSize.ToString());
            putRequest.Metadata.Add("ccmsdatetime", uploadDateTime.UtcDateTime.Ticks.ToString());

            var result = await client.PutObjectAsync(putRequest);

            return result.HttpStatusCode == HttpStatusCode.OK;
        }

        #endregion
    }
}