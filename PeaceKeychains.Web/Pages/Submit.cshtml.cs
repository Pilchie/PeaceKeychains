using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PeaceKeychains.Web.Models;

namespace PeaceKeychains.Web.Pages;

public class SubmitModel : PageModel
{
    private readonly PeaceKeychainsContext _dbContext;
    private readonly BlobServiceClient _blobClient;

    public SubmitModel(PeaceKeychainsContext dbContext, BlobServiceClient blobClient)
    {
        _dbContext = dbContext;
        _blobClient = blobClient;
    }

    public async Task<IActionResult> OnPost(string title, string user, string text, IFormFile image)
    {
        var now = DateTime.Now;
        var p = new Post(Guid.NewGuid(), now, title, user, text);

        if (image is not null)
        {
            using var imageStream = image.OpenReadStream();
            var containerClient = _blobClient.GetBlobContainerClient("images");
            var blockBlobClient = containerClient.GetBlockBlobClient($"{now:O}-{image.FileName}");
            var blobContentInfo = await blockBlobClient.UploadAsync(imageStream);
            var blobInfo = await blockBlobClient.SetHttpHeadersAsync(new BlobHttpHeaders { ContentType = image.ContentType });
            p.OriginalImageUrl = blockBlobClient.Uri.AbsoluteUri;
        }

        // TODO: Queue generation of other image sizes, and notify the need for moderation.
        p.Approved = true;

        _dbContext.Posts.Add(p);
        await _dbContext.SaveChangesAsync();

        return Redirect("/");
    }
}
