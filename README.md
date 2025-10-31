# Media Gallery

## Configuration

The application reads its database connection string and media root directory from the standard ASP.NET Core configuration system. The default values are stored in `MediaGallery.Web/appsettings.json`.

### Environment-specific overrides

You can override any value by creating an environment-specific file such as `MediaGallery.Web/appsettings.Development.json`. Values defined in `appsettings.{Environment}.json` replace the defaults when the application runs in the corresponding environment.

### User secrets for local development

For local development, store sensitive settings in the project's user secrets store. Run the following command from the `MediaGallery.Web` directory to open the secrets file:

```
dotnet user-secrets set "ConnectionStrings:Default" "Server=...;Database=...;User Id=...;Password=..."
dotnet user-secrets set "Media:RootDirectory" "D:\\MediaRoot"
```

These secrets override the JSON files while keeping credentials out of source control.
