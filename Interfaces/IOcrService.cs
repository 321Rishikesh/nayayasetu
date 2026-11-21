namespace NyayaSetu.Interfaces
{
    public record OcrAnalysisResult(string ExtractedText, string QualityNote, bool HasPotentialIssues);

    public interface IOcrService
    {
        Task<OcrAnalysisResult> AnalyzeDocumentAsync(string filePath, string contentType, string originalFileName);
    }
}
