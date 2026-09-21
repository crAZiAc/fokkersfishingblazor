using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FokkersFishing.Api.Models.Dtos;
using FokkersFishing.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace FokkersFishing.Api.Controllers
{
    [Authorize(Roles = "Administrator, User")]
    [ApiController]
    [Route("[controller]")]
    public class PhotoController : ControllerBase
    {
        private const string ContainerName = "catches";
        private readonly IFokkersDbService _fokkersDbService;

        public PhotoController(IFokkersDbService fokkersDbService)
        {
            _fokkersDbService = fokkersDbService;
        }

        [HttpPost]
        public async Task<ActionResult<UploadPhotoResponse>> UploadCatchPhotos(Photo photo)
        {
            var response = await UploadPhoto(photo);
            if (response == null) return BadRequest();
            return CreatedAtAction(nameof(UploadCatchPhotos), new { url = response.PhotoUrl }, response);
        }

        [HttpDelete("{id}")]
        public ActionResult<string> DeletePhotos(Guid id)
        {
            var client = new BlobContainerClient(_fokkersDbService.StorageConnectionString, ContainerName);
            var blobs = client.GetBlobs(BlobTraits.All, BlobStates.All, id.ToString()).ToList();
            if (blobs.Count == 0) return NotFound();
            foreach (var blob in blobs)
            {
                client.DeleteBlob(blob.Name, DeleteSnapshotsOption.IncludeSnapshots);
            }
            return NoContent();
        }

        private async Task<UploadPhotoResponse> UploadPhoto(Photo photo)
        {
            const string extension = "jpg";
            string imageName, imageThumbName;
            switch (photo.PhotoType)
            {
                case PhotoTypeEnum.Catch:
                    imageName = $"{photo.Id}-catch.{extension}";
                    imageThumbName = $"{photo.Id}-catch-tn.{extension}";
                    break;
                case PhotoTypeEnum.Measure:
                    imageName = $"{photo.Id}-measure.{extension}";
                    imageThumbName = $"{photo.Id}-measure-tn.{extension}";
                    break;
                default:
                    return null;
            }

            var container = new BlobContainerClient(_fokkersDbService.StorageConnectionString, ContainerName);
            container.CreateIfNotExists(PublicAccessType.Blob);

            var client = container.GetBlobClient(imageName);
            var clientThumbnail = container.GetBlobClient(imageThumbName);

            try
            {
                using (var image = Image.Load(photo.ImageContent))
                {
                    // Full image as JPEG.
                    using (var full = new MemoryStream())
                    {
                        image.Save(full, new JpegEncoder());
                        full.Position = 0;
                        await client.UploadAsync(full, new BlobUploadOptions
                        {
                            HttpHeaders = new BlobHttpHeaders { ContentType = "image/jpeg" }
                        });
                    }

                    // 64x64 thumbnail (matches original behaviour).
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(64, 64),
                        Mode = ResizeMode.Stretch
                    }));
                    using (var thumb = new MemoryStream())
                    {
                        image.Save(thumb, new JpegEncoder());
                        thumb.Position = 0;
                        await clientThumbnail.UploadAsync(thumb, new BlobUploadOptions
                        {
                            HttpHeaders = new BlobHttpHeaders { ContentType = "image/jpeg" }
                        });
                    }
                }
            }
            catch
            {
                return null;
            }

            return new UploadPhotoResponse
            {
                PhotoUrl = client.Uri.ToString(),
                ThumbnailUrl = clientThumbnail.Uri.ToString()
            };
        }
    }
}
