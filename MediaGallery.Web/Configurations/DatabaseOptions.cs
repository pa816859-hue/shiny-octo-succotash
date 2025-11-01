namespace MediaGallery.Web.Configurations;

public class DatabaseOptions
{
    public string Default { get; set; } = string.Empty;

    public int CommandTimeoutSeconds { get; set; } = 120;
}
