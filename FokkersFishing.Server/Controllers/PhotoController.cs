using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FokkersFishing.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FokkersFishing.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using FokkersFishing.Data;
using FokkersFishing.Shared.Models;
using FokkersFishing.Server.Helpers;
using Microsoft.AspNetCore.Identity;
using Azure.Storage.Blobs;
using System.IO;
using System.Drawing;

namespace FokkersFishing.Controllers
{
    [Authorize(Roles = "Administrator, User")]
    [ApiController]
    [Route("[controller]")]
    public class PhotoController : Controller
    {
        private readonly ILogger<PhotoController> _logger;
        private readonly IFokkersDbService _fokkersDbService;

        public PhotoController(ILogger<PhotoController> logger, IFokkersDbService fokkersDbService, IHttpContextAccessor httpContextAccessor, ApplicationDbContext dbContext)
        {
            _logger = logger;
            _fokkersDbService = fokkersDbService;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UploadPhotoResponse>> UploadCatchPhoto(Photo photo)
        {
            UploadPhotoResponse response = await UploadPhoto(photo);
            if (response == null)
            {
                return null;
            }
            return CreatedAtAction(nameof(UploadCatchPhoto), new { url = response.PhotoUrl }, response);
        }

        [HttpDelete("catch/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<string>> DeleteCatchPhoto(Guid id)
        {
            string imageNamePrefix = $"catch-{id.ToString()}";
            BlobContainerClient client = new BlobContainerClient(_fokkersDbService.StorageConnectionString, "catches");
            var blobs = client.GetBlobs(Azure.Storage.Blobs.Models.BlobTraits.All, Azure.Storage.Blobs.Models.BlobStates.All, imageNamePrefix);
            if (blobs.Count() == 0)
            {
                return NotFound();
            }
            else
            {
                foreach (var blob in blobs.ToList())
                {
                    client.DeleteBlob(blob.Name, Azure.Storage.Blobs.Models.DeleteSnapshotsOption.IncludeSnapshots);
                }
                return NoContent();
            }
        }

        [HttpDelete("measure/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<string>> DeleteMeasurePhoto(Guid id)
        {
            string imageNamePrefix = $"measure-{id.ToString()}";
            BlobContainerClient client = new BlobContainerClient(_fokkersDbService.StorageConnectionString, "catches");
            var blobs = client.GetBlobs(Azure.Storage.Blobs.Models.BlobTraits.All, Azure.Storage.Blobs.Models.BlobStates.All, imageNamePrefix);
            if (blobs.Count() == 0)
            {
                return NotFound();
            }
            else
            {
                foreach (var blob in blobs.ToList())
                {
                    client.DeleteBlob(blob.Name, Azure.Storage.Blobs.Models.DeleteSnapshotsOption.IncludeSnapshots);
                }
                return NoContent();
            }
        }

        #region Helpers
        protected Image GetReducedImage(int width, int height, Stream resourceImage)
        {
            try
            {
                var image = Image.FromStream(resourceImage);
                var thumb = image.GetThumbnailImage(width, height, () => false, IntPtr.Zero);

                return thumb;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        protected async Task<UploadPhotoResponse> UploadPhoto(Photo photo)
        {
            string imageName = string.Empty;
            string imageThumbName = string.Empty;
            switch (photo.PhotoType)
            {
                case PhotoTypeEnum.Catch:
                    {
                        imageName = $"catch-{photo.Id.ToString()}.png";
                        imageThumbName = $"tn-catch-{photo.Id.ToString()}.png";
                        break;
                    }
                case PhotoTypeEnum.Measure:
                    {
                        imageName = $"measure-{photo.Id.ToString()}.png";
                        imageThumbName = $"tn-measure-{photo.Id.ToString()}.png";
                        break;
                    }
            }
            
            BlobClient client = new BlobClient(_fokkersDbService.StorageConnectionString, "catches", imageName);
            BlobClient clientThumbnail = new BlobClient(_fokkersDbService.StorageConnectionString, "catches", imageThumbName);
            try
            {
                // upload full
                Image pngImage = null;
                using (Stream stream = new MemoryStream(photo.ImageContent))
                {
                    pngImage = Image.FromStream(stream);
                    
                }
                using (Stream stream = new MemoryStream())
                {
                    pngImage.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    stream.Position = 0;
                    await client.UploadAsync(stream, true);
                }

                // upload thumbnail
                Image thumbNailImage = null;
                using (Stream stream = new MemoryStream(photo.ImageContent))
                {
                    thumbNailImage = GetReducedImage(32, 32, stream);
                }
                Stream thumbStream = new MemoryStream();
                thumbNailImage.Save(thumbStream, System.Drawing.Imaging.ImageFormat.Png);
                thumbStream.Position = 0;
                await clientThumbnail.UploadAsync(thumbStream, true);
                thumbStream.Close();

            }
            catch (Exception ex)
            {
                return null;
            }
            return new UploadPhotoResponse { PhotoUrl = client.Uri.ToString(), ThumbnailUrl = clientThumbnail.Uri.ToString() };
        }
        #endregion

    } // end c
} // end ns
