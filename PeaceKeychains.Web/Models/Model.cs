using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace PeaceKeychains.Web.Models;

public class PeaceKeychainsContext : DbContext
{
    public PeaceKeychainsContext(DbContextOptions<PeaceKeychainsContext> options)
        : base(options)
    {
    }

    public DbSet<Post> Posts { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
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

    public bool Approved { get; set; }
}

public class Image
{
    public Image(Uri largeImage)
    {
        ArgumentNullException.ThrowIfNull(largeImage);

        LargeImage = largeImage;
    }

    public Uri LargeImage { get; init; }
}
