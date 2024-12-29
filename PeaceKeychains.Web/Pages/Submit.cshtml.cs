using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using ImageMagick;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PeaceKeychains.Web.Models;

namespace PeaceKeychains.Web.Pages;

public class SubmitModel(PeaceKeychainsContext dbContext, BlobServiceClient blobClient, ILogger<SubmitModel> logger) : PageModel
{
    public async Task<IActionResult> OnPost(string title, string user, string text, IFormFile image)
    {
        ModelState.Clear();
        if (string.IsNullOrWhiteSpace(title))
        {
            ModelState.AddModelError(nameof(title), "Please provide a title");
        }

        if (string.IsNullOrWhiteSpace(user))
        {
            ModelState.AddModelError(nameof(user), "Please provide a user");
        }

        if (string.IsNullOrWhiteSpace(text) && image is null)
        {
            ModelState.AddModelError(nameof(text), "Please provide either text or an image");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }
        else
        {
            var now = DateTime.Now;
            var p = new Post(Guid.NewGuid(), now, title, user, text);

            if (image is not null)
            {
                logger.LogInformation("Uploading image with FileName: {filename}, ContentType: {contentType}", image.FileName, image.ContentType);
                var containerClient = blobClient.GetBlobContainerClient("images");
                if (image.ContentType == "application/octet-stream" && Path.GetExtension(image.FileName) == ".heic")
                {
                    logger.LogInformation("Converting image to jpg.");

                    var fileName = Path.ChangeExtension(image.FileName, ".jpg");
                    var contentType = "image/jpeg";
                    var blockBlobClient = containerClient.GetBlockBlobClient($"{now:O}-{fileName}");
                    using var memStream = new MemoryStream();
                    using var imageStream = image.OpenReadStream();
                    using var magickImage = new MagickImage(imageStream);
                    magickImage.Format = MagickFormat.Jpeg;
                    var data = magickImage.ToByteArray();
                    using var convertedStream = new MemoryStream(data, false);
                    var blobContentInfo = await blockBlobClient.UploadAsync(convertedStream);
                    var blobInfo = await blockBlobClient.SetHttpHeadersAsync(new BlobHttpHeaders { ContentType = contentType });
                    p.OriginalImageUrl = blockBlobClient.Uri.AbsoluteUri;
                }
                else
                {
                    using var imageStream = image.OpenReadStream();
                    var blockBlobClient = containerClient.GetBlockBlobClient($"{now:O}-{image.FileName}");
                    var blobContentInfo = await blockBlobClient.UploadAsync(imageStream);
                    var blobInfo = await blockBlobClient.SetHttpHeadersAsync(new BlobHttpHeaders { ContentType = image.ContentType });
                    p.OriginalImageUrl = blockBlobClient.Uri.AbsoluteUri;
                }
            }

            // TODO: Queue generation of other image sizes, and notify the need for moderation.
            p.Approved = true;

            dbContext.Posts.Add(p);
            await dbContext.SaveChangesAsync();

            return Redirect("/");
        }
    }
}
