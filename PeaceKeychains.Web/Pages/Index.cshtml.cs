using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PeaceKeychains.Web.Models;

namespace PeaceKeychains.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly PeaceKeychainsContext _dbContext;

    public IndexModel(ILogger<IndexModel> logger, PeaceKeychainsContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task OnGet()
    {
        Posts = await _dbContext.Posts.Where(p => p.Approved).OrderByDescending(p => p.Time).Take(10).AsNoTracking().ToListAsync();
    }

    public List<Post>? Posts { get; private set; }
}
