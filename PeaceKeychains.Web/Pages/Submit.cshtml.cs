using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PeaceKeychains.Web.Models;

namespace PeaceKeychains.Web.Pages;

public class SubmitModel : PageModel
{
    private readonly PeaceKeychainsContext _dbContext;

    public SubmitModel(PeaceKeychainsContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IActionResult> OnPost(string title, string user, string text)
    {
        var p = new Post(Guid.NewGuid(), DateTime.Now, title, user, text);

        // TODO: remove this once moderation exists
        p.Approved = true;

        _dbContext.Posts.Add(p);
        await _dbContext.SaveChangesAsync();

        return Redirect("/");
    }
}
