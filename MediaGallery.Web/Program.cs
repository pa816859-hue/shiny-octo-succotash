using System;
using System.IO;
using MediaGallery.Web.Configurations;
using MediaGallery.Web.Infrastructure.Data;
using MediaGallery.Web.Services;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<DatabaseOptions>(builder.Configuration.GetSection("ConnectionStrings"));
builder.Services.Configure<MediaOptions>(builder.Configuration.GetSection("Media"));

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();
builder.Services.AddSingleton<ISqlCommandExecutor, SqlCommandExecutor>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPhotoRepository, PhotoRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<IVideoRepository, VideoRepository>();
builder.Services.AddScoped<IMediaFileProvider, MediaFileProvider>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddSingleton<IPhotoStateStore, PhotoStateStore>();
builder.Services.AddSingleton<IVideoStateStore, VideoStateStore>();
builder.Services.AddScoped<IPhotoService, PhotoService>();
builder.Services.AddScoped<IVideoService, VideoService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

var mediaOptions = app.Services.GetRequiredService<IOptions<MediaOptions>>();
var mediaRoot = mediaOptions.Value.RootDirectory;

if (string.IsNullOrWhiteSpace(mediaRoot))
{
    app.Logger.LogWarning("Media root directory is not configured. Media files will not be served.");
}
else
{
    try
    {
        var absoluteMediaRoot = Path.GetFullPath(mediaRoot);
        if (Directory.Exists(absoluteMediaRoot))
        {
            var contentTypeProvider = new FileExtensionContentTypeProvider();
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(absoluteMediaRoot),
                RequestPath = "/media",
                ServeUnknownFileTypes = true,
                ContentTypeProvider = contentTypeProvider
            });
        }
        else
        {
            app.Logger.LogWarning(
                "Configured media root '{MediaRoot}' does not exist. Media files will not be served.",
                absoluteMediaRoot);
        }
    }
    catch (Exception exception)
    {
        app.Logger.LogError(exception, "Failed to configure media file provider for root '{MediaRoot}'.", mediaRoot);
    }
}

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
