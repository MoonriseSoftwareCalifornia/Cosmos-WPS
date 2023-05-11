using System.Collections.Generic;
using System.IO;
using System.Web;
using Cosmos.BlobService.Models;

namespace Cosmos.BlobService
{
    /// <summary>
    ///     Utility functions
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        ///     Gets the content type of an uploaded file
        /// </summary>
        /// <param name="fileMetaData"></param>
        /// <returns></returns>
        public static string GetContentType(FileUploadMetaData fileMetaData)
        {
            if (!string.IsNullOrEmpty(fileMetaData.ContentType.Trim()) ||
                string.IsNullOrEmpty(fileMetaData.FileName.Trim())) return fileMetaData.ContentType;

            var extension = Path.GetExtension(fileMetaData.FileName)?.ToLower();

            if (string.IsNullOrEmpty(extension)) return "application/octet-stream";
            //svg as "image/svg+xml"(W3C: August 2011)
            switch (extension)
            {
                case ".ttf":
                    return "application/x-font-ttf"; // (IANA: March 2013)
                case ".woff":
                    return "application/font-woff"; // (IANA: January 2013)
                case ".woff2":
                    return "application/font-woff2"; // (W3C W./ E.Draft: May 2014 / March 2016)
                case ".or":
                    return "application/x-font-truetype";
                case ".otf":
                    return "application/x-font-opentype"; // (IANA: March 2013)
                case ".eot":
                    return "application/vnd.ms-fontobject"; // (IANA: December 2005)
                case ".sfnt":
                    return "application/font-sfnt"; // (IANA: March 2013)
                default:
                    return "application/octet-stream";
            }
        }

        /// <summary>
        ///     Encodes the file name for blob storage, and prepends the name with the folder path.
        /// </summary>
        /// <param name="folderName"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetBlobName(string folderName, string fileName)
        {
            return $"{folderName.TrimEnd('/')}/{fileName}";
        }

        /// <summary>
        ///     Parses out a path into a string array.
        /// </summary>
        /// <param name="pathParts"></param>
        /// <returns></returns>
        public static string[] ParsePath(params string[] pathParts)
        {
            if (pathParts == null) return new string[] { };

            var paths = new List<string>();

            foreach (var part in pathParts)
                if (!string.IsNullOrEmpty(part))
                {
                    var split = part.Split("/");
                    foreach (var p in split)
                        if (!string.IsNullOrEmpty(p))
                        {
                            var path = TrimPathPart(p);
                            if (!string.IsNullOrEmpty(path)) paths.Add(path);
                        }
                }

            return paths.ToArray();
        }

        /// <summary>
        ///     Trims leading and trailing slashes and white space from a path part.
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        public static string TrimPathPart(string part)
        {
            if (string.IsNullOrEmpty(part))
                return "";

            return part.Trim('/').Trim('\\').Trim();
        }

        /// <summary>
        ///     Encodes a URL
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <remarks>
        ///     For more information, see
        ///     <a
        ///         href="https://docs.microsoft.com/en-us/rest/api/storageservices/Naming-and-Referencing-Containers--Blobs--and-Metadata#blob-names">
        ///         documentation
        ///     </a>
        ///     .
        /// </remarks>
        public static string UrlEncode(string path)
        {
            var parts = ParsePath(path);
            var urlEncodedParts = new List<string>();
            foreach (var part in parts) urlEncodedParts.Add(HttpUtility.UrlEncode(part.Replace(" ", "-")));

            return TrimPathPart(string.Join('/', urlEncodedParts));
        }
    }
}