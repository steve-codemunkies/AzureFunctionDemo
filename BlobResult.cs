using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.Text;

namespace AzureFunctionStaticFiles
{
    /// <summary>
    /// Storage blob result.
    /// </summary>
    public class BlobResult : FileStreamResult
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public BlobResult(BlobDownloadInfo blob)
            : base(blob.Content, blob.ContentType)
        {
            EntityTag = new EntityTagHeaderValue(blob.Details.ETag.ToString());
        }
    }
}
