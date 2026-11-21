using System.Text;
using NyayaSetu.Interfaces;

namespace NyayaSetu.Services
{
    /// <summary>
    /// Smart Document Assistance OCR service.
    /// Performs lightweight built-in PDF text inspection and image quality heuristics,
    /// with a clean extension point for Tesseract or Azure AI Document Intelligence.
    /// Final document verification always remains with an authorized officer.
    /// </summary>
    public class OcrService : IOcrService
    {
        public async Task<OcrAnalysisResult> AnalyzeDocumentAsync(string filePath, string contentType, string originalFileName)
        {
            if (!File.Exists(filePath))
            {
                return new OcrAnalysisResult(string.Empty, "File not found for OCR inspection.", true);
            }

            var info = new FileInfo(filePath);
            if (info.Length < 1024)
            {
                return new OcrAnalysisResult(
                    string.Empty,
                    "Warning: File size is unusually small (<1 KB). Document may be blank or unreadable; manual officer review required.",
                    true);
            }

            if (contentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
            {
                var bytes = await File.ReadAllBytesAsync(filePath);
                var raw = Encoding.Latin1.GetString(bytes);
                var printable = new StringBuilder();
                int tokenCount = 0;

                // Lightweight scan for plain PDF text operators
                foreach (var ch in raw)
                {
                    if (ch >= 32 && ch <= 126)
                    {
                        printable.Append(ch);
                        if (char.IsLetterOrDigit(ch)) tokenCount++;
                    }
                    else if (ch == '\n' || ch == '\r')
                    {
                        printable.Append(' ');
                    }
                }

                var sample = printable.Length > 400 ? printable.ToString(0, 400) : printable.ToString();
                bool hasTextLayer = raw.Contains("/Font", StringComparison.Ordinal) && tokenCount > 80;
                var note = hasTextLayer
                    ? $"PDF structure verified ({info.Length / 1024} KB). Text layer detected; ready for officer verification."
                    : $"PDF appears to be a scanned image ({info.Length / 1024} KB). Connect Tesseract/Azure OCR for full text extraction; officer verification required.";

                return new OcrAnalysisResult(
                    hasTextLayer ? $"[Automated Preview - {originalFileName}] {sample}" : $"[Scanned PDF - {originalFileName}] No embedded text layer detected.",
                    note,
                    !hasTextLayer);
            }

            // Image document heuristic
            bool lowResSuspected = info.Length < 15 * 1024;
            var imgNote = lowResSuspected
                ? $"Image size is small ({info.Length / 1024} KB) and may have low legibility. Officer should verify clarity."
                : $"Image document ({info.Length / 1024} KB) passed basic legibility size checks. Ready for officer verification.";

            return new OcrAnalysisResult(
                $"[Image Document - {originalFileName}] Queued for officer visual verification.",
                imgNote,
                lowResSuspected);
        }
    }
}
