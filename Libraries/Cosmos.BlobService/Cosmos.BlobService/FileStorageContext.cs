using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.BlobService.Config;
using Cosmos.BlobService.Drivers;
using Cosmos.BlobService.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Cosmos.BlobService
{
    /// <summary>
    ///     Azure Files Share Context
    /// </summary>
    public sealed class FileStorageContext
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="sharename"></param>
        public FileStorageContext(string connectionString, string sharename)
        {
            _driver = new AzureFileStorage(connectionString, sharename);
        }

        /// <summary>
        ///     Determines if a blob exists.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<bool> BlobExistsAsync(string path)
        {
            return await _driver.BlobExistsAsync(path);
        }

        /// <summary>
        ///     Copies a file or folder.
        /// </summary>
        /// <param name="sourcePath">Path to source file or folder</param>
        /// <param name="destFolderPath">Path to destination folder</param>
        /// <returns></returns>
        public async Task CopyAsync(string sourcePath, string destFolderPath)
        {
            await _driver.CopyBlobAsync(sourcePath, destFolderPath);
        }

        /// <summary>
        ///     Delete a folder
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public async Task DeleteFolderAsync(string folder)
        {
            // Ensure leading slash is removed.
            await _driver.DeleteFolderAsync(folder);
        }

        /// <summary>
        ///     Deletes a file
        /// </summary>
        /// <param name="target"></param>
        public async Task DeleteFileAsync(string target)
        {
            // Ensure leading slash is removed.
            target = target.TrimStart('/');
            await _driver.DeleteIfExistsAsync(target);
        }

        /// <summary>
        ///     Gets a file
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public async Task<FileManagerEntry> GetFileAsync(string target)
        {
            // Ensure leading slash is removed.
            target = target.TrimStart('/');

            var fileManagerEntry = await _driver.GetBlobAsync(target);
            return fileManagerEntry;
        }


        /// <summary>
        ///     Moves a file or folder to a specified folder.
        /// </summary>
        /// <param name="sourcePath">Path to source file or folder</param>
        /// <param name="destFolderPath">Path to destination folder</param>
        /// <returns></returns>
        public async Task MoveAsync(string sourcePath, string destFolderPath)
        {
            await _driver.MoveAsync(sourcePath, destFolderPath);
        }

        /// <summary>
        ///     Returns a response stream from the primary blob storage provider.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public async Task<Stream> OpenBlobReadStreamAsync(string target)
        {
            // Ensure leading slash is removed.
            target = target.TrimStart('/');

            return await _driver.GetStreamAsync(target);

        }

        /// <summary>
        ///     Renames (and can move) a file or folder.
        /// </summary>
        /// <param name="sourcePath">Full path to item to change</param>
        /// <param name="destinationPath">Full path to the new name</param>
        /// <returns></returns>
        public async Task RenameAsync(string sourcePath, string destinationPath)
        {
            await _driver.RenameAsync(sourcePath, destinationPath);
        }

        #region PRIVATE FIELDS AND METHODS

        // private readonly IOptions<CosmosStorageConfig> _config;

        /// <summary>
        /// Azure file share driver, this is not handled in the collection.
        /// </summary>
        private readonly AzureFileStorage _driver;

        private async Task<List<string>> GetBlobNamesByPath(string path, string[] filter = null)
        {
            return await _driver.GetBlobNamesByPath(path, filter);
        }

        private string[] ParseFirstFolder(string path)
        {
            var parts = path.Split('/');
            return parts;
        }

        #endregion

        #region FILE MANAGER FUNCTION

        /// <summary>
        ///     Append bytes to blob(s)
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="fileMetaData"></param>
        /// <returns></returns>
        public async Task AppendBlob(MemoryStream stream, FileUploadMetaData fileMetaData)
        {
            await _driver.AppendBlobAsync(stream.ToArray(), fileMetaData, DateTimeOffset.UtcNow);
        }

        /// <summary>
        ///     Creates a folder in all the cloud storage accounts
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns></returns>
        /// <remarks>Creates the folder if it does not already exist.</remarks>
        public async Task<FileManagerEntry> CreateFolder(string folderName)
        {
            await _driver.CreateFolderAsync(folderName);
            var folder = await _driver.GetBlobAsync(folderName);
            return folder;
        }

        public async Task<FileManagerEntry> GetObjectAsync(string path)
        {
            var item = await _driver.GetObjectAsync(path);
            return item;
        }

        /// <summary>
        ///     Gets files and subfolders for a given path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<List<FileManagerEntry>> GetObjectsAsync(string path)
        {
            if (!string.IsNullOrEmpty(path)) path = path.TrimStart('/');
            var entries = await _driver.GetObjectsAsync(path);
            return entries;
        }

        /// <summary>
        ///     Gets the contents for a folder
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public async Task<List<FileManagerEntry>> GetFolderContents(string folder)
        {
            if (!string.IsNullOrEmpty(folder))
            {
                folder = folder.TrimStart('/');

                if (folder == "/")
                {
                    folder = "";
                }
                else
                {
                    if (!folder.EndsWith("/")) folder = folder + "/";
                }
            }

            return await GetObjectsAsync(folder);
        }

        #endregion

    }

}