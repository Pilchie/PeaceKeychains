using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PeaceKeychains.Shared.Data;
using PeaceKeychains.Shared.Extensions;

if (args.Length == 0)
{
    PrintUsage();
    return 1;
}

var command = args[0].ToLowerInvariant();
if (command is not ("check" or "moderate"))
{
    Console.Error.WriteLine($"Unknown command: '{args[0]}'");
    PrintUsage();
    return 1;
}

var vaultUri = Environment.GetEnvironmentVariable("VaultUri");
if (string.IsNullOrWhiteSpace(vaultUri))
{
    Console.Error.WriteLine("Error: Must set 'VaultUri' environment variable.");
    return 1;
}

var configuration = new ConfigurationBuilder()
    .AddAzureKeyVault(new Uri(vaultUri), new DefaultAzureCredential())
    .Build();

var cosmosConnectionString = configuration["ConnectionStrings:AzureCosmosDBConnectionString"]
    ?? throw new InvalidOperationException("KeyVault must contain 'ConnectionStrings:AzureCosmosDBConnectionString'");

var storageConnectionString = configuration["AzureStorageConnectionString"]
    ?? throw new InvalidOperationException("KeyVault must contain 'AzureStorageConnectionString'");

var services = new ServiceCollection();
services.AddCosmos<PeaceKeychainsContext>(cosmosConnectionString, "PostsDB");
services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddBlobServiceClient(storageConnectionString, preferMsi: false);
});

using var serviceProvider = services.BuildServiceProvider();
using var scope = serviceProvider.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<PeaceKeychainsContext>();
var blobServiceClient = scope.ServiceProvider.GetRequiredService<BlobServiceClient>();

return command switch
{
    "check" => await CheckAsync(dbContext),
    "moderate" => await ModerateAsync(dbContext, blobServiceClient),
    _ => 1
};

static async Task<int> CheckAsync(PeaceKeychainsContext dbContext)
{
    var count = await dbContext.Posts
        .Where(p => !p.Approved)
        .CountAsync();

    if (count == 0)
    {
        Console.WriteLine("No unapproved posts.");
    }
    else
    {
        Console.WriteLine($"{count} unapproved post(s) found.");
    }

    return Math.Min(count, 125);
}

static async Task<int> ModerateAsync(PeaceKeychainsContext dbContext, BlobServiceClient blobServiceClient)
{
    var posts = await dbContext.Posts
        .Where(p => !p.Approved)
        .OrderBy(p => p.Time)
        .ToListAsync();

    if (posts.Count == 0)
    {
        Console.WriteLine("No unapproved posts to moderate.");
        return 0;
    }

    Console.WriteLine($"Found {posts.Count} unapproved post(s).\n");

    var approved = 0;
    var deleted = 0;
    var skipped = 0;

    for (var i = 0; i < posts.Count; i++)
    {
        var post = posts[i];
        Console.WriteLine($"--- Post {i + 1} of {posts.Count} ---");
        Console.WriteLine($"  Title:    {post.Title}");
        Console.WriteLine($"  User:     {post.UserName}");
        Console.WriteLine($"  Time:     {post.Time:g}");
        Console.WriteLine($"  Text:     {post.Text ?? "(none)"}");
        Console.WriteLine($"  Image:    {post.OriginalImageUrl ?? "(none)"}");
        Console.WriteLine();

        var action = PromptAction();
        switch (action)
        {
            case 'a':
                post.Approved = true;
                dbContext.Posts.Update(post);
                await dbContext.SaveChangesAsync();
                Console.WriteLine("  -> Post approved.\n");
                approved++;
                break;

            case 'd':
                if (!string.IsNullOrEmpty(post.OriginalImageUrl))
                {
                    await DeleteBlobAsync(blobServiceClient, post.OriginalImageUrl);
                }

                dbContext.Posts.Remove(post);
                await dbContext.SaveChangesAsync();
                Console.WriteLine("  -> Post deleted.\n");
                deleted++;
                break;

            case 's':
                Console.WriteLine("  -> Skipped.\n");
                skipped++;
                break;
        }
    }

    Console.WriteLine("--- Summary ---");
    Console.WriteLine($"  Approved: {approved}");
    Console.WriteLine($"  Deleted:  {deleted}");
    Console.WriteLine($"  Skipped:  {skipped}");

    return 0;
}

static char PromptAction()
{
    while (true)
    {
        Console.Write("[A]pprove / [D]elete / [S]kip? ");
        var key = Console.ReadKey(intercept: false);
        Console.WriteLine();

        var c = char.ToLowerInvariant(key.KeyChar);
        if (c is 'a' or 'd' or 's')
        {
            return c;
        }

        Console.WriteLine("  Invalid choice. Please press A, D, or S.");
    }
}

static async Task DeleteBlobAsync(BlobServiceClient blobServiceClient, string imageUrl)
{
    try
    {
        var uri = new Uri(imageUrl);

        // Path is "/{container}/{blobName}" — strip leading slash and split
        var pathSegments = uri.AbsolutePath.TrimStart('/').Split('/', 2);
        if (pathSegments.Length != 2)
        {
            Console.WriteLine($"  Warning: Could not parse blob path from URL: {imageUrl}");
            return;
        }

        var containerName = pathSegments[0];
        var blobName = pathSegments[1];

        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        var deleted = await blobClient.DeleteIfExistsAsync();
        if (deleted)
        {
            Console.WriteLine($"  -> Blob deleted: {blobName}");
        }
        else
        {
            Console.WriteLine($"  -> Blob not found: {blobName}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  Warning: Failed to delete blob: {ex.Message}");
    }
}

static void PrintUsage()
{
    Console.WriteLine("Usage: PeaceKeychains.Admin <command>");
    Console.WriteLine();
    Console.WriteLine("Commands:");
    Console.WriteLine("  check       Check for unapproved posts (exit code = count, max 125)");
    Console.WriteLine("  moderate    Interactively review unapproved posts");
}
