using System.Drawing.Printing;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PeaceKeychains.Web.Models;

namespace PeaceKeychains.Web.Pages;

public class IndexModel : PageModel
{
    public const int PageSize = 10;
    private readonly ILogger<IndexModel> _logger;
    private readonly PeaceKeychainsContext _dbContext;

    public IndexModel(ILogger<IndexModel> logger, PeaceKeychainsContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task OnGet(int? p)
    {
        Count = await _dbContext.Posts.CountAsync();
        Current = p == null ? 0 : p.Value;
        Posts = await _dbContext.Posts.Where(p => p.Approved).OrderByDescending(p => p.Time).Skip(Current* PageSize).Take(PageSize).AsNoTracking().ToListAsync();
    }

    public List<Post>? Posts { get; private set; }

    public int Count { get; private set; }

    public int Current { get; private set; }

}
