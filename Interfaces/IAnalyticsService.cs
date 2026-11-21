using NyayaSetu.ViewModels;

namespace NyayaSetu.Interfaces
{
    public interface IAnalyticsService
    {
        Task<AdminAnalyticsViewModel> GetAdminAnalyticsAsync();
    }
}
