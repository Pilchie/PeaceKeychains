# Copilot Instructions for PeaceKeychains

## Build & Run

```bash
# Restore + install TailwindCSS deps (required before first build)
dotnet restore
cd PeaceKeychains.Blazor && npm install && cd ..

# Build the full solution
dotnet build

# Run the Blazor app (the active frontend)
dotnet run --project PeaceKeychains.Blazor

# Run the legacy Razor Pages app
dotnet run --project PeaceKeychains.Web
```

There are no test projects in this solution. `dotnet test` will succeed but discover nothing.

## Architecture

This is a .NET 8 solution for sharing photos and stories about Peace Keychains, deployed to Azure App Service at peacekeychains.com.

Three projects:

- **PeaceKeychains.Shared** — Class library containing the `Post` model, EF Core `DbContext` (Cosmos DB), and Azure Blob Storage extension methods. Both web projects reference this.
- **PeaceKeychains.Blazor** — Blazor interactive server app with TailwindCSS. This is the active/modern frontend.
- **PeaceKeychains.Web** — Legacy Razor Pages frontend (Bootstrap-based). Shares the same data layer but has less robust validation than the Blazor app.

### Data flow

- **Database**: Azure Cosmos DB via EF Core (`PeaceKeychainsContext`, database: `PostsDB`)
- **Image storage**: Azure Blob Storage (`images` container). Images are uploaded with a timestamp-prefixed filename.
- **Secrets**: Azure Key Vault, loaded at startup via `VaultUri` environment variable and `DefaultAzureCredential`

### Post submission pipeline

1. Form input validated (title, username, optional text, optional image)
2. Image file validated by extension whitelist (`.jpg`, `.jpeg`, `.png`, `.gif`, `.webp`, `.heic`) and magic byte signatures
3. HEIC images auto-converted to JPEG via ImageMagick (Magick.NET)
4. Image uploaded to Azure Blob Storage
5. Post saved to Cosmos DB
6. Honeypot field ("Website") controls auto-approval: filled = not approved

## Key Conventions

### C# style (enforced via .editorconfig)

- File-scoped namespaces (`namespace X;`)
- `var` everywhere (all three `csharp_style_var_*` rules are set)
- Always use braces, even for single-line blocks
- Primary constructors for DI (e.g., `class Foo(IService svc)`)
- Nullable reference types enabled across all projects
- Allman-style braces (open brace on new line)

### TailwindCSS (Blazor project only)

TailwindCSS v4 compiles as a pre-build MSBuild target. The input file is `PeaceKeychains.Blazor/Styles/app.css` and output goes to `wwwroot/css/app.css`.

Custom theme colors defined in the input CSS:
- `peace-*` (green palette) and `calm-*` (blue palette)

For live CSS development: `cd PeaceKeychains.Blazor && npm run watch:css`

### Blazor components

Pages live in `PeaceKeychains.Blazor/Components/Pages/`. Layout is in `Components/Layout/MainLayout.razor`. Pages use route attributes like `@page "/{p:int?}"` for pagination.
