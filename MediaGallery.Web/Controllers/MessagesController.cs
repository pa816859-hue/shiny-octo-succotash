using System;
using System.Threading;
using System.Threading.Tasks;
using MediaGallery.Web.Services;
using MediaGallery.Web.Services.Models;
using MediaGallery.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace MediaGallery.Web.Controllers;

public class MessagesController : Controller
{
    private readonly IMessageService _messageService;

    public MessagesController(IMessageService messageService)
    {
        _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
    }

    [HttpGet]
    public async Task<IActionResult> Recent(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = PaginationHelper.DefaultPageSize,
        [FromQuery] long? channelId = null,
        [FromQuery] long? userId = null,
        [FromQuery] bool mediaOnly = false,
        [FromQuery] MessageSortOrder sortOrder = MessageSortOrder.NewestFirst,
        CancellationToken cancellationToken = default)
    {
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = PaginationHelper.ClampPageSize(pageSize);

        var viewModel = await _messageService
            .GetRecentMessagesAsync(normalizedPage, normalizedPageSize, channelId, userId, mediaOnly, sortOrder, cancellationToken)
            .ConfigureAwait(false);

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> RecentData(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = PaginationHelper.DefaultPageSize,
        [FromQuery] long? channelId = null,
        [FromQuery] long? userId = null,
        [FromQuery] bool mediaOnly = false,
        [FromQuery] MessageSortOrder sortOrder = MessageSortOrder.NewestFirst,
        CancellationToken cancellationToken = default)
    {
        if (!PaginationHelper.TryValidate(page, pageSize, out var errorMessage))
        {
            return BadRequest(new { error = errorMessage });
        }

        var normalizedPageSize = PaginationHelper.ClampPageSize(pageSize);

        var viewModel = await _messageService
            .GetRecentMessagesAsync(page, normalizedPageSize, channelId, userId, mediaOnly, sortOrder, cancellationToken)
            .ConfigureAwait(false);

        var result = new PagedResult<RecentMessage>(viewModel.Messages, viewModel.Pagination);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> MediaChronology(
        [FromQuery] long? photoId = null,
        [FromQuery] long? videoId = null,
        CancellationToken cancellationToken = default)
    {
        var hasPhoto = photoId.HasValue;
        var hasVideo = videoId.HasValue;

        if (!hasPhoto && !hasVideo)
        {
            return BadRequest(new { error = "Provide a photoId or a videoId to view its chronology." });
        }

        if (hasPhoto && hasVideo)
        {
            return BadRequest(new { error = "Provide only one media identifier at a time." });
        }

        var viewModel = await _messageService
            .GetMediaChronologyAsync(photoId, videoId, cancellationToken)
            .ConfigureAwait(false);

        return View(viewModel);
    }
}
