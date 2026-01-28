using Microsoft.EntityFrameworkCore;
using PeaceKeychains.Shared.Models;

namespace PeaceKeychains.Shared.Data;

public class PeaceKeychainsContext(DbContextOptions<PeaceKeychainsContext> options) : DbContext(options)
{
    public DbSet<Post> Posts { get; set; } = null!;
}
