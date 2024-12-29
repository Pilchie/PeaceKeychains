using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PeaceKeychains.Web.Models;

namespace PeaceKeychains.Web.Pages;

public class IndexModel(PeaceKeychainsContext dbContext) : PageModel
{
    public const int PageSize = 10;

    public async Task OnGet(int? p)
    {
        Count = await dbContext.Posts.CountAsync();
        Current = p == null ? 0 : p.Value;
        Posts = await dbContext.Posts.Where(p => p.Approved).OrderByDescending(p => p.Time).Skip(Current * PageSize).Take(PageSize).AsNoTracking().ToListAsync();
    }

    public List<Post>? Posts { get; private set; }

    public int Count { get; private set; }

    public int Current { get; private set; }

}
