
using Cosmos.BlobService;
using Cosmos.BlobService.Models;
using Cosmos.Common.Data;
using Cosmos.Cms.Common.Services.Configurations;
using Cosmos.Cms.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeTypes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Cosmos.Cms.Controllers
{
    /// <summary>
    /// API Controller
    /// </summary>
    [AllowAnonymous]
    //[ResponseCache(NoStore = true)]
    [Authorize(Roles = "Administrators, Editors, Authors")]
    public class CodeController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<CodeController> _logger;
        private readonly FileStorageContext _storageContext;
        private readonly IOptions<CosmosConfig> _options;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cosmosConfig"></param>
        /// <param name="dbContext"></param>
        /// <param name="logger"></param>
        /// <param name="storageContext"></param>
        public CodeController(IOptions<CosmosConfig> cosmosConfig,
            ApplicationDbContext dbContext, ILogger<CodeController> logger, FileStorageContext storageContext)
        {
            _dbContext = dbContext;
            _logger = logger;
            _storageContext = storageContext;
            _options = cosmosConfig;
        }

        private static long DivideByAndRoundUp(long number, long divideBy)
        {
            return (long)Math.Ceiling((float)number / (float)divideBy);
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
        public string UrlEncode(string path)
        {
            var parts = ParsePath(path);
            var urlEncodedParts = new List<string>();
            foreach (var part in parts) urlEncodedParts.Add(HttpUtility.UrlEncode(part.Replace(" ", "-")).Replace("%40", "@"));

            return TrimPathPart(string.Join('/', urlEncodedParts));
        }

        /// <summary>
        ///     Parses out a path into a string array.
        /// </summary>
        /// <param name="pathParts"></param>
        /// <returns></returns>
        public string[] ParsePath(params string[] pathParts)
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
        public string TrimPathPart(string part)
        {
            if (string.IsNullOrEmpty(part))
                return "";

            return part.Trim('/').Trim('\\').Trim();
        }

        #region FILE MANAGER FUNCTIONS

        /// <summary>
        /// Moves items to a new folder.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Move(MoveFilesViewModel model)
        {
            try
            {
                foreach (var item in model.Items)
                {
                    string dest;

                    if (item.EndsWith("/"))
                    {
                        // moving a directory
                        dest = model.Destination + item.TrimEnd('/').Split('/').LastOrDefault();
                    }
                    else
                    {
                        // moving a file
                        dest = model.Destination + item.Split('/').LastOrDefault();
                    }

                    await _storageContext.RenameAsync(item, dest);
                }

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            return Ok();
        }

        /// <summary>
        /// New folder action
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewFolder(NewFolderViewModel model)
        {
            var relativePath = string.Join('/', ParsePath(model.ParentFolder, model.FolderName));
            relativePath = UrlEncode(relativePath);

            // Check for duplicate entries
            var existingEntries = await _storageContext.GetFolderContents(model.ParentFolder);

            if (existingEntries.Any(f => f.Name.Equals(model.FolderName)) == false)
            {
                var fileManagerEntry = _storageContext.CreateFolder(relativePath);
            }

            return RedirectToAction("Source", new { target = model.ParentFolder, directoryOnly = model.DirectoryOnly });

        }

        /// <summary>
        /// Download a file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<IActionResult> Download(string path)
        {
            var blob = await _storageContext.GetFileAsync(path);

            if (!blob.IsDirectory)
            {
                using var stream = await _storageContext.OpenBlobReadStreamAsync(path);
                using var memStream = new MemoryStream();
                stream.CopyTo(memStream);
                return File(memStream.ToArray(), "application/octet-stream", fileDownloadName: blob.Name);
            }

            return NotFound();
        }

        /// <summary>
        ///     Creates a new entry, using relative paths, and normalizes entry name to lower case.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="entry"></param>
        /// <returns><see cref="JsonResult" />(<see cref="BlobService.FileManagerEntry" />)</returns>
        public async Task<ActionResult> Create(string target, BlobService.FileManagerEntry entry)
        {
            target = target == null ? "" : target;
            entry.Path = target;
            entry.Name = UrlEncode(entry.Name);
            entry.Extension = entry.Extension;

            if (!entry.Path.StartsWith("/pub", StringComparison.CurrentCultureIgnoreCase))
            {
                return Unauthorized("New folders can't be created here using this tool. Please select the 'pub' folder and try again.");
            }

            // Check for duplicate entries
            var existingEntries = await _storageContext.GetFolderContents(target);

            if (existingEntries != null && existingEntries.Any())
            {
                var results = existingEntries.FirstOrDefault(f => f.Name.Equals(entry.Name));

                if (results != null)
                {
                    //var i = 1;
                    var originalName = entry.Name;
                    for (var i = 0; i < existingEntries.Count; i++)
                    {
                        entry.Name = originalName + "-" + (i + 1);
                        if (!existingEntries.Any(f => f.Name.Equals(entry.Name))) break;
                        i++;
                    }
                }
            }

            var relativePath = string.Join('/', ParsePath(entry.Path, entry.Name));
            relativePath = UrlEncode(relativePath);

            var fileManagerEntry = _storageContext.CreateFolder(relativePath);

            return Json(fileManagerEntry);
        }

        /// <summary>
        ///     Deletes a folder, normalizes entry to lower case.
        /// </summary>
        /// <param name="model">Item to delete using relative path</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> Delete(DeleteBlobItemsViewModel model)
        {
            foreach (var item in model.Paths)
            {
                if (item.EndsWith('/'))
                {
                    await _storageContext.DeleteFolderAsync(item.TrimEnd('/'));
                }
                else
                {
                    await _storageContext.DeleteFileAsync(item);
                }
            }

            return Ok();
        }

        /// <summary>
        /// Rename a blob item.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rename(RenameBlobViewModel model)
        {
            if (!string.IsNullOrEmpty(model.ToBlobName))
            {
                // Note rules:
                // 1. New folder names must end with slash.
                // 2. New file names must never end with a slash.
                if (model.FromBlobName.EndsWith("/"))
                {
                    if (!model.ToBlobName.EndsWith("/"))
                    {
                        model.ToBlobName = model.ToBlobName + "/";
                    }
                }
                else
                {
                    model.ToBlobName = model.ToBlobName.TrimEnd('/');
                }

                var target = $"{model.BlobRootPath}/{model.FromBlobName}";

                var dest = $"{model.BlobRootPath}/{UrlEncode(model.ToBlobName)}";

                await _storageContext.RenameAsync(target, dest);
            }

            return Ok();
        }

        /// <summary>
        /// Source files inventory
        /// </summary>
        /// <param name="target"></param>
        /// <param name="sortOrder"></param>
        /// <param name="currentSort"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <param name="directoryOnly"></param>
        /// <param name="selectOne"></param>
        /// <param name="imagesOnly"></param>
        /// <param name="isNewSession"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Index(string target, string sortOrder = "asc", string currentSort = "Name", int pageNo = 0, int pageSize = 10, bool directoryOnly = false, bool selectOne = false, bool imagesOnly = false, bool isNewSession = false)
        {
            target = string.IsNullOrEmpty(target) ? "" : HttpUtility.UrlDecode(target);

            ViewData["PathPrefix"] = target.StartsWith('/') ? target : "/" + target;
            ViewData["DirectoryOnly"] = directoryOnly;
            ViewData["Container"] = null;
            ViewData["Title"] = "Internal File Manager";
            ViewData["StorageName"] = "Internal File Storage";
            ViewData["TopDirectory"] = "/";
            ViewData["Controller"] = "Code";
            ViewData["Action"] = "Index";
            ViewData["SelectOne"] = selectOne;
            ViewData["ImagesOnly"] = imagesOnly;
            ViewData["isNewSession"] = isNewSession;

            //
            // Grid pagination
            // 
            ViewData["sortOrder"] = sortOrder;
            ViewData["currentSort"] = currentSort;
            ViewData["pageNo"] = pageNo;
            ViewData["pageSize"] = pageSize;

            //
            // GET FULL OR ABSOLUTE PATH
            //
            var model = await _storageContext.GetFolderContents(target);


            var query = model.AsQueryable();

            ViewData["RowCount"] = query.Count();

            if (sortOrder == "desc")
            {
                if (!string.IsNullOrEmpty(currentSort))
                {
                    switch (currentSort)
                    {
                        case "Name":
                            query = query.OrderByDescending(o => o.Name);
                            break;
                        case "IsDirectory":
                            query = query.OrderByDescending(o => o.IsDirectory);
                            break;
                        case "CreatedUtc":
                            query = query.OrderByDescending(o => o.CreatedUtc);
                            break;
                        case "Extension":
                            query = query.OrderByDescending(o => o.Extension);
                            break;
                        case "ModifiedUtc":
                            query = query.OrderByDescending(o => o.ModifiedUtc);
                            break;
                        case "Path":
                            query = query.OrderByDescending(o => o.Path);
                            break;
                        case "Size":
                            query = query.OrderByDescending(o => o.Size);
                            break;
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(currentSort))
                {
                    switch (currentSort)
                    {
                        case "Name":
                            query = query.OrderBy(o => o.Name);
                            break;
                        case "IsDirectory":
                            query = query.OrderBy(o => o.IsDirectory);
                            break;
                        case "CreatedUtc":
                            query = query.OrderBy(o => o.CreatedUtc);
                            break;
                        case "Extension":
                            query = query.OrderBy(o => o.Extension);
                            break;
                        case "ModifiedUtc":
                            query = query.OrderBy(o => o.ModifiedUtc);
                            break;
                        case "Path":
                            query = query.OrderBy(o => o.Path);
                            break;
                        case "Size":
                            query = query.OrderBy(o => o.Size);
                            break;
                    }
                }
            }
            if (directoryOnly)
            {
                return View(model.Where(w => w.IsDirectory == true).ToList());
            }

            return View("~/Views/Shared/FileExplorer/Index.cshtml", model.Skip(pageNo * pageSize).Take(pageSize).ToList());
        }

        /// <summary>
        /// Gets a unique GUID for FilePond
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Process([FromForm] string files)
        {
            var parsed = JsonConvert.DeserializeObject<FilePondMetadata>(files);

            var mime = MimeTypeMap.GetMimeType(Path.GetExtension(parsed.FileName));

            var uid = $"{parsed.Path.TrimEnd('/')}|{parsed.RelativePath.TrimStart('/')}|{Guid.NewGuid().ToString()}|{mime}";

            return Ok(uid);
        }

        /// <summary>
        /// Process a chunked upload
        /// </summary>
        /// <param name="patch"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        [HttpPatch]
        public async Task<ActionResult> Process(string patch, string options = "")
        {
            try
            {
                var patchArray = patch.Split('|');

                // Mime type
                var contentType = patchArray[3];

                // 0 based index
                var uploadOffset = long.Parse(Request.Headers["Upload-Offset"]);

                // File name being uploaded
                var UploadName = ((string)Request.Headers["Upload-Name"]);

                // Total size of the file in bytes
                var uploadLenth = long.Parse(Request.Headers["Upload-Length"]);

                // Size of the chunk
                var contentSize = long.Parse(Request.Headers["Content-Length"]);

                long chunk = 0;

                if (uploadOffset > 0)
                {
                    chunk = DivideByAndRoundUp(uploadLenth, uploadOffset);
                }

                var totalChunks = DivideByAndRoundUp(uploadLenth, contentSize);


                var blobName = UrlEncode(UploadName);

                var relativePath = UrlEncode(patchArray[0].TrimEnd('/'));

                if (!string.IsNullOrEmpty(patchArray[1]))
                {
                    var dpath = Path.GetDirectoryName(patchArray[1]).Replace('\\', '/'); // Convert windows paths to unix style.
                    var epath = UrlEncode(dpath);
                    relativePath += "/" + UrlEncode(epath);
                }

                var metaData = new FileUploadMetaData()
                {
                    ChunkIndex = chunk,
                    ContentType = contentType,
                    FileName = blobName,
                    RelativePath = relativePath + "/" + blobName,
                    TotalChunks = totalChunks,
                    TotalFileSize = uploadLenth,
                    UploadUid = patchArray[1]
                };

                // Make sure full folder path exists
                var pathParts = patchArray[0].Trim('/').Split('/');
                var part = "";
                for (int i = 0; i < pathParts.Length - 1; i++)
                {
                    if (i == 0 && pathParts[i] != "pub")
                    {
                        throw new Exception("Must upload folders and files under /pub directory.");
                    }

                    part = $"{part}/{pathParts[i]}";
                    if (part != "/pub")
                    {
                        var folder = part.Trim('/');
                        await _storageContext.CreateFolder(folder);
                    }
                }

                using var memoryStream = new MemoryStream();
                await Request.Body.CopyToAsync(memoryStream);

                await _storageContext.AppendBlob(memoryStream, metaData);

            }
            catch (Exception e)
            {
                //var t = e; // For debugging
                _logger.LogError(e.Message, e);
                throw;
            }


            return Ok();
        }

        #endregion

        #region CODE EDITOR METHODS

        /// <summary>
        /// Edit code for a file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<IActionResult> EditCode(string path)
        {
            try
            {
                var extension = Path.GetExtension(path.ToLower());

                var filter = _options.Value.SiteSettings.AllowedFileTypes.Split(',');
                var editorField = new EditorField
                {
                    FieldId = "Content",
                    FieldName = Path.GetFileName(path)
                };

                if (!filter.Contains(extension)) return new UnsupportedMediaTypeResult();

                switch (extension)
                {
                    case ".js":
                        editorField.EditorMode = EditorMode.JavaScript;
                        editorField.IconUrl = "/images/seti-ui/icons/javascript.svg";
                        break;
                    case ".html":
                        editorField.EditorMode = EditorMode.Html;
                        editorField.IconUrl = "/images/seti-ui/icons/html.svg";
                        break;
                    case ".css":
                        editorField.EditorMode = EditorMode.Css;
                        editorField.IconUrl = "/images/seti-ui/icons/css.svg";
                        break;
                    case ".xml":
                        editorField.EditorMode = EditorMode.Xml;
                        editorField.IconUrl = "/images/seti-ui/icons/javascript.svg";
                        break;
                    case ".json":
                        editorField.EditorMode = EditorMode.Json;
                        editorField.IconUrl = "/images/seti-ui/icons/javascript.svg";
                        break;
                    default:
                        editorField.EditorMode = EditorMode.Html;
                        editorField.IconUrl = "/images/seti-ui/icons/html.svg";
                        break;
                }

                //
                // Get the blob now, so we can determine the type, or use this client as-is
                //
                //var properties = blob.GetProperties();

                // Open a stream
                await using var memoryStream = new MemoryStream();

                await using (var stream = await _storageContext.OpenBlobReadStreamAsync(path))
                {
                    // Load into memory and release the blob stream right away
                    await stream.CopyToAsync(memoryStream);
                }

                var metaData = await _storageContext.GetFileAsync(path);


                ViewData["PageTitle"] = metaData.Name;
                ViewData[" Published"] = DateTimeOffset.FromFileTime(metaData.ModifiedUtc.Ticks);

                return View(new FileManagerEditCodeViewModel
                {
                    Id = path,
                    Path = path,
                    EditorTitle = Path.GetFileName(Path.GetFileName(path)),
                    EditorFields = new List<EditorField>
                    {
                        editorField
                    },
                    Content = Encoding.UTF8.GetString(memoryStream.ToArray()),
                    EditingField = "Content",
                    CustomButtons = new List<string>()
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
                throw;
            }
        }

        /// <summary>
        /// Save the file
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCode(FileManagerEditCodeViewModel model)
        {

            var extension = Path.GetExtension(model.Path.ToLower());

            var filter = _options.Value.SiteSettings.AllowedFileTypes.Split(',');
            var editorField = new EditorField
            {
                FieldId = "Content",
                FieldName = Path.GetFileName(model.Path)
            };

            if (!filter.Contains(extension)) return new UnsupportedMediaTypeResult();

            var contentType = string.Empty;

            switch (extension)
            {
                case ".js":
                    editorField.EditorMode = EditorMode.JavaScript;
                    editorField.IconUrl = "/images/seti-ui/icons/javascript.svg";
                    break;
                case ".html":
                    editorField.EditorMode = EditorMode.Html;
                    editorField.IconUrl = "/images/seti-ui/icons/html.svg";
                    break;
                case ".css":
                    editorField.EditorMode = EditorMode.Css;
                    editorField.IconUrl = "/images/seti-ui/icons/css.svg";
                    break;
                case ".xml":
                    editorField.EditorMode = EditorMode.Xml;
                    editorField.IconUrl = "/images/seti-ui/icons/javascript.svg";
                    break;
                case ".json":
                    editorField.EditorMode = EditorMode.Json;
                    editorField.IconUrl = "/images/seti-ui/icons/javascript.svg";
                    break;
                default:
                    editorField.EditorMode = EditorMode.Html;
                    editorField.IconUrl = "/images/seti-ui/icons/html.svg";
                    break;
            }

            // Save the blob now

            var bytes = Encoding.Default.GetBytes(model.Content);

            using var memoryStream = new MemoryStream(bytes, false);

            var formFile = new FormFile(memoryStream, 0, memoryStream.Length, Path.GetFileNameWithoutExtension(model.Path), Path.GetFileName(model.Path));

            var metaData = new FileUploadMetaData
            {
                ChunkIndex = 0,
                ContentType = contentType,
                FileName = Path.GetFileName(model.Path),
                RelativePath = Path.GetFileName(model.Path),
                TotalFileSize = memoryStream.Length,
                UploadUid = Guid.NewGuid().ToString(),
                TotalChunks = 1
            };

            var uploadPath = model.Path.TrimEnd(metaData.FileName.ToArray()).TrimEnd('/');

            var result = (JsonResult)await Upload(new IFormFile[] { formFile }, JsonConvert.SerializeObject(metaData), uploadPath);

            var resultMode = (FileUploadResult)result.Value;

            var jsonModel = new SaveCodeResultJsonModel
            {
                ErrorCount = ModelState.ErrorCount,
                IsValid = ModelState.IsValid
            };

            if (!resultMode.uploaded)
            {
                ModelState.AddModelError("", $"Error saving {Path.GetFileName(model.Path)}");
            }

            jsonModel.Errors.AddRange(ModelState.Values
                .Where(w => w.ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid)
                .ToList());
            jsonModel.ValidationState = ModelState.ValidationState;

            return Json(jsonModel);
        }


        /// <summary>
        ///     Used to upload files, one chunk at a time, sets the correct MIME type, and normalizes the blob name to lower case.
        /// </summary>
        /// <param name="files"></param>
        /// <param name="metaData"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        [HttpPost]
        [RequestSizeLimit(
            6291456)] // AWS S3 multi part upload requires 5 MB parts--no more, no less so pad the upload size by a MB just in case
        public async Task<ActionResult> Upload(IEnumerable<IFormFile> files,
            string metaData, string path)
        {

            try
            {
                if (files == null || files.Any() == false)
                    return Json("");

                if (string.IsNullOrEmpty(path) || path.Trim('/') == "") return Unauthorized("Cannot upload here. Please select the 'pub' folder first, or sub-folder below that, then try again.");

                //
                // Get information about the chunk we are on.
                //
                var ms = new MemoryStream(Encoding.UTF8.GetBytes(metaData));

                var serializer = new JsonSerializer();
                FileUploadMetaData fileMetaData;
                using (var streamReader = new StreamReader(ms))
                {
                    fileMetaData =
                        (FileUploadMetaData)serializer.Deserialize(streamReader, typeof(FileUploadMetaData));
                }

                if (fileMetaData == null) throw new Exception("Could not read the file's metadata");

                var file = files.FirstOrDefault();

                if (file == null) throw new Exception("No file found to upload.");

                var blobName = UrlEncode(fileMetaData.FileName);

                fileMetaData.ContentType = MimeTypeMap.GetMimeType(Path.GetExtension(fileMetaData.FileName));
                fileMetaData.FileName = blobName;
                fileMetaData.RelativePath = (path.TrimEnd('/') + "/" + fileMetaData.RelativePath);

                // Make sure full folder path exists
                var parts = fileMetaData.RelativePath.Trim('/').Split('/');
                var part = "";
                for (int i = 0; i < parts.Length - 1; i++)
                {
                    part = $"{part}/{parts[i]}";
                    var folder = part.Trim('/');
                    await _storageContext.CreateFolder(folder);
                }

                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);

                await _storageContext.DeleteFileAsync(fileMetaData.RelativePath);

                await _storageContext.AppendBlob(memoryStream, fileMetaData);

                var fileBlob = new FileUploadResult
                {
                    uploaded = fileMetaData.TotalChunks - 1 <= fileMetaData.ChunkIndex,
                    fileUid = fileMetaData.UploadUid
                };
                return Json(fileBlob);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                throw ex;
            }
        }

        #endregion

    }
}
