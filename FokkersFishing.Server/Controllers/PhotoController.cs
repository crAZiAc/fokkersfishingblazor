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
        public async Task<ActionResult<UploadPhotoResponse>> UploadCatchPhotos(Photo photo)
        {
            UploadPhotoResponse response = await UploadPhoto(photo);
            if (response == null)
            {
                return null;
            }
            return CreatedAtAction(nameof(UploadCatchPhotos), new { url = response.PhotoUrl }, response);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<string>> DeletePhotos(Guid id)
        {
            string imageNamePrefix = $"{id.ToString()}";
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
            string extension = "jpg";
            switch (photo.PhotoType)
            {
                case PhotoTypeEnum.Catch:
                    {
                        imageName = $"{photo.Id.ToString()}-catch.{extension}";
                        imageThumbName = $"{photo.Id.ToString()}-catch-tn.{extension}";
                        break;
                    }
                case PhotoTypeEnum.Measure:
                    {
                        imageName = $"{photo.Id.ToString()}-measure.{extension}";
                        imageThumbName = $"{photo.Id.ToString()}-measure-tn.{extension}";
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
                    using (Stream imageStream = new MemoryStream())
                    {
                        pngImage.Save(imageStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                        imageStream.Position = 0;
                        await client.UploadAsync(imageStream, true);
                    }
                }

                // upload thumbnail
                Image thumbNailImage = null;
                using (Stream stream = new MemoryStream(photo.ImageContent))
                {
                    thumbNailImage = GetReducedImage(64, 64, stream);
                    using (Stream thumbStream = new MemoryStream())
                    {
                        thumbNailImage.Save(thumbStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                        thumbStream.Position = 0;
                        await clientThumbnail.UploadAsync(thumbStream, true);
                    }
                }
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
