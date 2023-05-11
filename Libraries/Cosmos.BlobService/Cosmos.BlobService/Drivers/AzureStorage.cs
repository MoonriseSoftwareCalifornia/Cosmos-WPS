using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Cosmos.BlobService.Config;
using Cosmos.BlobService.Models;

namespace Cosmos.BlobService.Drivers
{
    /// <summary>
    ///     Azure blob storage driver
    /// </summary>
    public sealed class AzureStorage : ICosmosStorage
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="config"></param>
        /// <param name="containerName"></param>
        public AzureStorage(AzureStorageConfig config, string containerName = "$web")
        {
            _containerName = containerName;
            _blobServiceClient = new BlobServiceClient(config.AzureBlobStorageConnectionString);
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
            var appendClient = GetAppendBlobClient(fileMetaData.RelativePath);


            if (fileMetaData.ChunkIndex == 0)
            {
                //await DeleteIfExistsAsync(fileMetaData.RelativePath);
                await appendClient.DeleteIfExistsAsync();

                var headers = new BlobHttpHeaders
                {
                    // Set the MIME ContentType every time the properties 
                    // are updated or the field will be cleared
                    ContentType = Utilities.GetContentType(fileMetaData)
                };

                await appendClient.CreateIfNotExistsAsync(headers);

                var dictionaryObject = new Dictionary<string, string>();
                dictionaryObject.Add("ccmsuploaduid", fileMetaData.UploadUid);
                dictionaryObject.Add("ccmssize", fileMetaData.TotalFileSize.ToString());
                dictionaryObject.Add("ccmsdatetime", uploadDateTime.UtcDateTime.Ticks.ToString());

                _ = await appendClient.SetMetadataAsync(dictionaryObject);
            }


            //
            // AWS Multi part upload requires parts or chunks to be 5MB, which
            // are too big for Azure append blobs, so buffer the upload size here.
            //

            await using Stream loadMemoryStream = new MemoryStream(data);

            loadMemoryStream.Position = 0;
            int bytesRead;
            var buffer = new byte[2621440]; // 2.5 MB 
            while ((bytesRead = loadMemoryStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                //Stream stream = new MemoryStream(buffer);
                var newArray = new Span<byte>(buffer, 0, bytesRead).ToArray();
                Stream stream = new MemoryStream(newArray);
                stream.Position = 0;
                await appendClient.AppendBlockAsync(stream);
            }

            if (fileMetaData.TotalChunks - 1 == fileMetaData.ChunkIndex)
            {
                // This is the last chunk, wrap things up here.
                await appendClient.SealAsync();
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

            return await blob.ExistsAsync();
        }

        /// <summary>
        ///     Copies a single blob and returns it's <see cref="BlobClient" />.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <returns>The destination or new <see cref="BlobClient" />.</returns>
        /// <remarks>
        ///     Tip: After operation, check the returned blob object to see if it exists.
        /// </remarks>
        public async Task CopyBlobAsync(string source, string destination)
        {
            source = source.TrimStart('/');
            destination = destination.TrimStart('/');

            var containerClient =
                _blobServiceClient.GetBlobContainerClient(_containerName);
            var sourceBlob = containerClient.GetBlobClient(source);
            if (await sourceBlob.ExistsAsync())
            {
                var lease = sourceBlob.GetBlobLeaseClient();
                await lease.AcquireAsync(TimeSpan.FromSeconds(-1));

                // Get a BlobClient representing the destination blob with a unique name.
                var destBlob = containerClient.GetBlobClient(destination);

                // Start the copy operation.
                var c = await destBlob.StartCopyFromUriAsync(sourceBlob.Uri);
                await c.WaitForCompletionAsync();

                await lease.ReleaseAsync();
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

            var blobClient = await GetBlobAsync(fullPath);

            if (await blobClient.ExistsAsync() == false)
            {
                var byteArray = Encoding.ASCII.GetBytes($"This is a folder stub file for {path}.");
                await using var stream = new MemoryStream(byteArray);
                await blobClient.UploadAsync(stream);
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
            var containerClient =
                _blobServiceClient.GetBlobContainerClient(_containerName);

            var pageable = containerClient.GetBlobsAsync(prefix: path).AsPages();

            var results = new List<BlobItem>();
            await foreach (var page in pageable)
            {
                results.AddRange(page.Values);
            }

            return results.Select(s => s.Name).ToList();
        }

        /// <summary>
        ///     Deletes a folder and all its contents.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public async Task<int> DeleteFolderAsync(string target)
        {
            var blobs = await GetBlobItemsByPath(target);
            var containerClient =
                _blobServiceClient.GetBlobContainerClient(_containerName);

            var responses = new List<Response<bool>>();

            foreach (var blob in blobs) responses.Add(await containerClient.DeleteBlobIfExistsAsync(blob.Name));

            return responses.Count(r => r.Value);
        }

        /// <summary>
        ///     Deletes a file (blob).
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public async Task DeleteIfExistsAsync(string target)
        {
            var containerClient =
                _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.DeleteBlobIfExistsAsync(target, DeleteSnapshotsOption.IncludeSnapshots);
        }

        /// <summary>
        /// Enables the static website and sets default CORS rule
        /// </summary>
        /// <returns></returns>
        public async Task EnableStaticWebsite()
        {
            BlobServiceProperties properties = await _blobServiceClient.GetPropertiesAsync();

            if (!properties.StaticWebsite.Enabled)
            {
                properties.StaticWebsite.Enabled = true;

                if (properties.Cors == null)
                {
                    var corsRule = new BlobCorsRule();
                    corsRule.AllowedMethods = "GET,HEAD,OPTIONS";
                    corsRule.AllowedOrigins = "*";
                    corsRule.AllowedHeaders = "*";
                    corsRule.ExposedHeaders = "*";
                    properties.Cors = new List<BlobCorsRule>() { corsRule };
                }

                _blobServiceClient.SetProperties(properties);
            }
        }

        /// <summary>
        ///     Gets a client for a blob.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public async Task<BlobClient> GetBlobAsync(string target)
        {
            if (string.IsNullOrEmpty(target)) return null;

            target = target.TrimStart('/');
            var containerClient =
                _blobServiceClient.GetBlobContainerClient(_containerName);

            if (await containerClient.ExistsAsync()) return containerClient.GetBlobClient(target);

            return null;
        }

        /// <summary>
        /// Get blob itmes by path.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public async Task<List<BlobItem>> GetBlobItemsByPath(string target)
        {
            var results = new List<BlobItem>();
            var containerClient =
                _blobServiceClient.GetBlobContainerClient(_containerName);
            var items = containerClient.GetBlobsAsync(prefix: target);

            await foreach (var item in items) results.Add(item);

            return results;
        }

        /// <summary>
        /// Gets metadata for a blob
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public async Task<FileMetadata> GetFileMetadataAsync(string target)
        {
            var blobClient = await GetBlobAsync(target);
            var properties = await blobClient.GetPropertiesAsync();
            _ = long.TryParse(properties.Value.Metadata["ccmsuploaduid"], out var mark);
            return new FileMetadata()
            {
                ContentLength = properties.Value.ContentLength,
                ContentType = properties.Value.ContentType,
                ETag = properties.Value.ETag.ToString(),
                FileName = blobClient.Name,
                LastModified = properties.Value.LastModified.UtcDateTime,
                UploadDateTime = mark
            };
        }

        public Task<List<FileMetadata>> GetInventory()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Gets files and subfolders for a given path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<List<BlobHierarchyItem>> GetObjectsAsync(string path)
        {
            if (path == "/") path = "";

            if (!string.IsNullOrEmpty(path)) path = path.TrimStart('/');

            var containerClient =
                _blobServiceClient.GetBlobContainerClient(_containerName);


            var resultSegment = containerClient.GetBlobsByHierarchyAsync(prefix: path, delimiter: "/")
                .AsPages();

            var results = new List<BlobHierarchyItem>();

            await foreach (var blobPage in resultSegment)
            {
                results.AddRange(blobPage.Values);
            }

            return results;
        }

        /// <summary>
        /// Opens a read stream to download the blob.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public async Task<Stream> GetStreamAsync(string target)
        {
            var containerClient =
                _blobServiceClient.GetBlobContainerClient(_containerName);

            var blobClient = containerClient.GetAppendBlobClient(target);

            return await blobClient.OpenReadAsync(new BlobOpenReadOptions(false));
        }


        /// <summary>
        /// Uploads file to a stream
        /// </summary>
        /// <param name="readStream"></param>
        /// <param name="fileMetaData"></param>
        /// <param name="uploadDateTime"></param>
        /// <returns></returns>
        public async Task<bool> UploadStreamAsync(Stream readStream, FileUploadMetaData fileMetaData, DateTimeOffset uploadDateTime)
        {

            var appendClient = GetAppendBlobClient(fileMetaData.RelativePath);
            await appendClient.DeleteIfExistsAsync();


            //BlobProperties properties = await appendClient.GetPropertiesAsync();

            var headers = new BlobHttpHeaders
            {
                // Set the MIME ContentType every time the properties 
                // are updated or the field will be cleared
                ContentType = Utilities.GetContentType(fileMetaData)
            };
            await appendClient.CreateIfNotExistsAsync(headers);

            var dictionaryObject = new Dictionary<string, string>();
            dictionaryObject.Add("ccmsuploaduid", fileMetaData.UploadUid);
            dictionaryObject.Add("ccmssize", fileMetaData.TotalFileSize.ToString());
            dictionaryObject.Add("ccmsdatetime", uploadDateTime.UtcDateTime.Ticks.ToString());

            _ = await appendClient.SetMetadataAsync(dictionaryObject);

            var writeStream = await appendClient.OpenWriteAsync(false);

            await readStream.CopyToAsync(writeStream);

            return true;
        }

        #region PRIVATE FIELDS AND METHODS

        private readonly string _containerName;
        private readonly BlobServiceClient _blobServiceClient;

        /// <summary>
        ///     Gets an append blob client, used for chunk uploads.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        private AppendBlobClient GetAppendBlobClient(string target)
        {
            var containerClient =
                _blobServiceClient.GetBlobContainerClient(_containerName);

            return containerClient.GetAppendBlobClient(target);
        }


        //private async Task<List<BlobItem>> ListBlobsHierarchicalListing(string startsWith, int? segmentSize,
        //    BlobContainerClient container = null)
        //{
        //    var blobItemList = new List<BlobItem>();

        //    // If the container is given, we are likely reusing one in a search.
        //    // Otherwise if null, get the default container.
        //    container ??= _blobServiceClient.GetBlobContainerClient("$web");

        //    // Call the listing operation and return pages of the specified size.
        //    var resultSegment = container.GetBlobsByHierarchyAsync(prefix: startsWith, delimiter: "/")
        //        .AsPages(default, segmentSize);

        //    // Enumerate the blobs returned for each page.
        //    await foreach (var blobPage in resultSegment)
        //        // A hierarchical listing may return both virtual directories and blobs.
        //        foreach (var blobHierarchyItem in blobPage.Values)
        //            if (blobHierarchyItem.IsPrefix)
        //            {
        //                // Write out the prefix of the virtual directory.
        //                //Console.WriteLine("Virtual directory prefix: {0}", blobHierarchyItem.Prefix);

        //                // Call recursively with the prefix to traverse the virtual directory.
        //                blobItemList.AddRange(
        //                    await ListBlobsHierarchicalListing(blobHierarchyItem.Prefix, null, container));
        //            }
        //            else
        //            {
        //                // Write out the name of the blob.
        //                blobItemList.Add(blobHierarchyItem.Blob);
        //            }

        //    // Return the blob list here.
        //    return blobItemList;
        //}

        #endregion
    }
}