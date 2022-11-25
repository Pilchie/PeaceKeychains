using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using PeaceKeychains.Web.Models;

var builder = WebApplication.CreateBuilder(args);

var keyVaultEndpoint = new Uri(Environment.GetEnvironmentVariable("VaultUri"));
builder.Configuration.AddAzureKeyVault(keyVaultEndpoint, new DefaultAzureCredential());

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddBlobServiceClient(builder.Configuration["AzureStorageConnectionString:blob"], preferMsi: true);
    clientBuilder.AddQueueServiceClient(builder.Configuration["AzureStorageConnectionString:queue"], preferMsi: true);
});
builder.Services.AddCosmos<PeaceKeychainsContext>(builder.Configuration["ConnectionStrings:AzureCosmosDBConnectionString"], "PostsDB");

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

using (var scope = app.Services.CreateScope())
using (var dbContext = scope.ServiceProvider.GetRequiredService<PeaceKeychainsContext>())
{
    await dbContext.Database.EnsureCreatedAsync();
    if (!(await dbContext.Posts.Take(1).ToListAsync()).Any())
    {
        var p = new Post(Guid.NewGuid(), DateTime.Now, "First Post title", "Pilchie", "This is a sample post to see if it works");
        p.Approved = true;
        dbContext.Posts.Add(p);
        await dbContext.SaveChangesAsync();
    }
}

app.Run();
