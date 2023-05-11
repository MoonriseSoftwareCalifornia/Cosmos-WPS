using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Cosmos.BlobService.Models;

namespace Cosmos.BlobService.Drivers
{
    public interface ICosmosStorage
    {
        /// <summary>
        ///     Appends byte array to blob(s)
        /// </summary>
        /// <param name="data"></param>
        /// <param name="fileMetaData"></param>
        /// <param name="uploadDateTime"></param>
        /// <returns></returns>
        Task AppendBlobAsync(byte[] data, FileUploadMetaData fileMetaData, DateTimeOffset uploadDateTime);

        /// <summary>
        ///     Checks to see if a blob exists.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<bool> BlobExistsAsync(string path);

        /// <summary>
        /// </summary>
        /// Copies a blob
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        Task CopyBlobAsync(string source, string destination);

        /// <summary>
        ///     Creates a folder
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Task CreateFolderAsync(string path);

        /// <summary>
        ///     Deletes a file (blob).
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        Task DeleteIfExistsAsync(string target);

        /// <summary>
        ///     Deletes a folder an all its contents
        /// </summary>
        /// <param name="target"></param>
        /// <returns>Number of files deleted</returns>
        Task<int> DeleteFolderAsync(string target);

        /// <summary>
        ///     Gets a list of blob names for a given path (is recursive).
        /// </summary>
        /// <param name="path"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<List<string>> GetBlobNamesByPath(string path, string[] filter = null);

        /// <summary>
        /// Gets an inventory of all blobs in a storage account
        /// </summary>
        /// <returns></returns>
        Task<List<FileMetadata>> GetInventory();

        /// <summary>
        /// Gets metadata for a blob
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        Task<FileMetadata>GetFileMetadataAsync(string target);

        /// <summary>
        /// Opens a read stream from the blob in storage
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        Task<Stream> GetStreamAsync(string target);

        /// <summary>
        /// Uploads file to a stream
        /// </summary>
        /// <param name="readStream"></param>
        /// <param name="fileMetaData"></param>
        /// <param name="uploadDateTime"></param>
        /// <returns></returns>
        Task<bool> UploadStreamAsync(Stream readStream, FileUploadMetaData fileMetaData, DateTimeOffset uploadDateTime);
    }
}