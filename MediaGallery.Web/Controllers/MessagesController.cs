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
        [FromQuery] int pageSize = PaginationHelper.DefaultPageSizeLimit,
        [FromQuery] long? channelId = null,
        [FromQuery] long? userId = null,
        [FromQuery] bool mediaOnly = false,
        [FromQuery] MessageSortOrder sortOrder = MessageSortOrder.NewestFirst,
        CancellationToken cancellationToken = default)
    {
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = PaginationHelper.ClampPageSize(pageSize);

        var feed = await _messageService
            .GetRecentMessagesAsync(normalizedPage, normalizedPageSize, channelId, userId, mediaOnly, sortOrder, cancellationToken)
            .ConfigureAwait(false);

        var query = new MessageQueryViewModel(normalizedPage, normalizedPageSize, channelId, userId, mediaOnly, sortOrder);
        var viewModel = new RecentMessagesPageViewModel(feed, query);

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> RecentData(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = PaginationHelper.DefaultPageSizeLimit,
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
}
