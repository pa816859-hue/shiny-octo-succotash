using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaGallery.Web.Services;
using MediaGallery.Web.Services.Models;
using MediaGallery.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace MediaGallery.Web.Controllers;

public class HomeController : Controller
{
    private const int FeaturedPageSize = 12;

    private readonly IMessageService _messageService;
    private readonly ITagService _tagService;

    public HomeController(IMessageService messageService, ITagService tagService)
    {
        _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
        _tagService = tagService ?? throw new ArgumentNullException(nameof(tagService));
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var recentMedia = await _messageService
            .GetRecentMessagesAsync(1, FeaturedPageSize, channelId: null, userId: null, mediaOnly: true, MessageSortOrder.NewestFirst, cancellationToken)
            .ConfigureAwait(false);

        if (!recentMedia.Messages.Any())
        {
            recentMedia = await _messageService
                .GetRecentMessagesAsync(1, FeaturedPageSize, channelId: null, userId: null, mediaOnly: false, MessageSortOrder.NewestFirst, cancellationToken)
                .ConfigureAwait(false);
        }

        var tagSummaries = await _tagService
            .GetTagIndexAsync(1, FeaturedPageSize, userId: null, cancellationToken)
            .ConfigureAwait(false);

        var viewModel = new HomeDashboardViewModel(recentMedia, tagSummaries, DateTime.UtcNow);
        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
