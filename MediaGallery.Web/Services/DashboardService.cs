using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaGallery.Web.Infrastructure.Data;
using MediaGallery.Web.Services.Mapping;
using MediaGallery.Web.ViewModels;

namespace MediaGallery.Web.Services;

public sealed class DashboardService : IDashboardService
{
    private const int DefaultRecentMessages = 9;
    private const int DefaultTopTags = 8;
    private const int DefaultRecentUsers = 6;

    private readonly IMessageRepository _messageRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IUserRepository _userRepository;
    private readonly IDashboardRepository _dashboardRepository;

    public DashboardService(
        IMessageRepository messageRepository,
        ITagRepository tagRepository,
        IUserRepository userRepository,
        IDashboardRepository dashboardRepository)
    {
        _messageRepository = messageRepository;
        _tagRepository = tagRepository;
        _userRepository = userRepository;
        _dashboardRepository = dashboardRepository;
    }

    public async Task<DashboardViewModel> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var summaryTask = _dashboardRepository.GetSummaryAsync(cancellationToken);
        var messagesTask = _messageRepository.GetRecentMessagesAsync(0, DefaultRecentMessages, null, null, sortAscending: false, mediaOnly: true, cancellationToken);
        var tagsTask = _tagRepository.GetTagSummariesAsync(0, DefaultTopTags, null, cancellationToken);
        var usersTask = _userRepository.GetUsersAsync(0, DefaultRecentUsers, cancellationToken);

        await Task.WhenAll(summaryTask, messagesTask, tagsTask, usersTask).ConfigureAwait(false);

        var summary = summaryTask.Result.ToDashboardSummary();
        var messages = messagesTask.Result.Select(message => message.ToRecentMessage()).ToList();
        var tags = tagsTask.Result.ToPhotoTagSummaries();
        var users = usersTask.Result.Select(user => user.ToUserSummary()).ToList();

        return new DashboardViewModel(summary, messages, tags, users);
    }
}
