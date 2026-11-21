namespace NyayaSetu.Interfaces
{
    public interface ILocalizationService
    {
        string Get(string key, string? lang = null);
        string NormalizeLanguage(string? lang);
    }
}
