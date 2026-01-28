using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using PeaceKeychains.Blazor.Components;
using PeaceKeychains.Shared.Data;
using PeaceKeychains.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure Azure Key Vault
var vaultUri = Environment.GetEnvironmentVariable("VaultUri") ?? throw new InvalidOperationException("Must set 'VaultUri' environment variable");
var keyVaultEndpoint = new Uri(vaultUri);
builder.Configuration.AddAzureKeyVault(keyVaultEndpoint, new DefaultAzureCredential());

// Add services to the container.
builder.Services.AddRazorComponents();

// Configure Azure Blob Storage
builder.Services.AddAzureClients(clientBuilder =>
{
    var storageConnectionString = builder.Configuration["AzureStorageConnectionString"] ?? throw new InvalidOperationException("KeyVault must contain 'AzureStorageConnectionString'");
    clientBuilder.AddBlobServiceClient(storageConnectionString, preferMsi: false);
});

// Configure Cosmos DB
var cosmosDBConnectionString = builder.Configuration["ConnectionStrings:AzureCosmosDBConnectionString"] ?? throw new InvalidOperationException("KeyVault must contain 'ConnectionStrings:AzureCosmosDBConnectionString'");
builder.Services.AddCosmos<PeaceKeychainsContext>(cosmosDBConnectionString, "PostsDB");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>();

app.Run();
