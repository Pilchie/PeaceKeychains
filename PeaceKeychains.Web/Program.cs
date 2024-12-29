using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using PeaceKeychains.Web.Models;

var builder = WebApplication.CreateBuilder(args);
var vaultUri = Environment.GetEnvironmentVariable("VaultUri") ?? throw new InvalidOperationException("Must set 'VaultUri' environment variable");
var keyVaultEndpoint = new Uri(vaultUri);
builder.Configuration.AddAzureKeyVault(keyVaultEndpoint, new DefaultAzureCredential());

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddAzureClients(clientBuilder =>
{
    var storageConnectionString = builder.Configuration["AzureStorageConnectionString"] ?? throw new InvalidOperationException("KeyVault must contain 'AzureStorageConnectionString'");
    clientBuilder.AddBlobServiceClient(storageConnectionString, preferMsi: false);
    //clientBuilder.AddQueueServiceClient(builder.Configuration["AzureStorageConnectionString:queue"], preferMsi: true);
});
var cosmosDBConnectionString = builder.Configuration["ConnectionStrings:AzureCosmosDBConnectionString"] ?? throw new InvalidOperationException("KeyVault must contain 'ConnectionStrings:AzureCosmosDBConnectionString'");
builder.Services.AddCosmos<PeaceKeychainsContext>(cosmosDBConnectionString, "PostsDB");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

var populateDatabase = false;
if (populateDatabase)
{
    using (var scope = app.Services.CreateScope())
    using (var dbContext = scope.ServiceProvider.GetRequiredService<PeaceKeychainsContext>())
    {
        await dbContext.Database.EnsureCreatedAsync();
        if (!(await dbContext.Posts.Take(1).ToListAsync()).Any())
        {
            var p = new Post(Guid.NewGuid(), DateTime.Now, "First Post title", "Pilchie", "This is a sample post to see if it works")
            {
                Approved = true
            };
            dbContext.Posts.Add(p);
            await dbContext.SaveChangesAsync();
        }
    }
}

app.Run();
