using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PeaceKeychains.Web.Models;

public class PeaceKeychainsContext(DbContextOptions<PeaceKeychainsContext> options) : DbContext(options)
{
    public DbSet<Post> Posts { get; set; } = null!;
}

public class Post
{
    public Post(Guid id, DateTime time, string title, string userName, string? text)
    {
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(userName);

        Id = id;
        Time = time;
        Title = title;
        UserName = userName;
        Text = text;
    }

    [Key]
    public Guid Id { get; init; }

    public DateTime Time { get; init; }

    public string Title { get; init; }

    public string UserName { get; init; }

    public string? Text { get; init; }

    public string? LargeImageUrl { get; set; }

    public string? ThumbNailImageUrl { get; set; }

    public string? OriginalImageUrl { get; set; }

    public bool Approved { get; set; }

    [NotMapped]
    public IFormFile? File { get; set; }
}
