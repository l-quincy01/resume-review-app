using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace ResumeReview.Api.Services.AtsEngine;

public sealed class PdfPigResumeTextExtractor : IResumeTextExtractor
{
    private const double SameLineTolerance = 3.0;
    private const double ColumnGapThreshold = 72.0;

    public Task<string> ExtractTextAsync(
        Stream pdfStream,
        CancellationToken cancellationToken = default)
    {
        if (pdfStream.CanSeek)
        {
            pdfStream.Position = 0;
        }

        using var document = PdfDocument.Open(pdfStream);
        var text = new StringBuilder();

        foreach (var page in document.GetPages())
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var line in ReconstructLines(page))
            {
                text.AppendLine(line);
            }
        }

        return Task.FromResult(text.ToString());
    }

    private static IEnumerable<string> ReconstructLines(Page page)
    {
        var rows = page.GetWords()
            .GroupBy(word => Math.Round(word.BoundingBox.Bottom / SameLineTolerance) * SameLineTolerance)
            .OrderByDescending(group => group.Key);

        foreach (var row in rows)
        {
            var segment = new List<Word>();
            Word? previousWord = null;

            foreach (var word in row.OrderBy(word => word.BoundingBox.Left))
            {
                if (previousWord is not null &&
                    word.BoundingBox.Left - previousWord.BoundingBox.Right > ColumnGapThreshold)
                {
                    yield return JoinWords(segment);
                    segment.Clear();
                }

                segment.Add(word);
                previousWord = word;
            }

            if (segment.Count > 0)
            {
                yield return JoinWords(segment);
            }
        }
    }

    private static string JoinWords(IEnumerable<Word> words)
    {
        return string.Join(" ", words.Select(word => word.Text)).Trim();
    }
}
