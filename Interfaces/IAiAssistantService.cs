namespace NyayaSetu.Interfaces
{
    public record AssistantResourceLink(string Title, string Url);
    public record AssistantReply(string Answer, List<AssistantResourceLink> Sources, string Disclaimer);

    public interface IAiAssistantService
    {
        AssistantReply Ask(string question, string language = "en");
    }
}
