using System.Text.RegularExpressions;
using ResumeReview.Api.Dtos.Responses;

namespace ResumeReview.Api.Services.AtsEngine;

public sealed class StandardHeaderValidator : IStandardHeaderValidator
{
    private const int RequiredHeaderPoints = 20;
    private const int OptionalHeaderPoints = 5;
    private const int WeakRequiredHeaderPoints = 15;
    private const int WeakOptionalHeaderPoints = 4;
    private const int UnclearHeaderPenalty = -5;

    private static readonly string[] RequiredHeaders =
    [
        "Summary",
        "Skills",
        "Work Experience",
        "Education"
    ];

    private static readonly string[] OptionalHeaders =
    [
        "Projects",
        "Certifications",
        "Awards",
        "Languages",
        "Volunteer Experience"
    ];

    private static readonly Dictionary<string, string> StrongHeaderAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Professional Summary"] = "Summary",
        ["Career Summary"] = "Summary",
        ["Technical Skills"] = "Skills",
        ["Core Skills"] = "Skills",
        ["Key Skills"] = "Skills",
        ["Skills & Technologies"] = "Skills",
        ["Professional Experience"] = "Work Experience",
        ["Employment History"] = "Work Experience",
        ["Career Experience"] = "Work Experience",
        ["Relevant Experience"] = "Work Experience",
        ["Academic Background"] = "Education",
        ["Education & Qualifications"] = "Education",
        ["Technical Projects"] = "Projects",
        ["Selected Projects"] = "Projects",
        ["Licences & Certifications"] = "Certifications",
        ["Certificates"] = "Certifications",
        ["Awards & Honours"] = "Awards"
    };

    private static readonly Dictionary<string, string> WeakHeaderAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Profile"] = "Summary",
        ["Objective"] = "Summary",
        ["Experiences"] = "Work Experience"
    };

    private static readonly HashSet<string> CanonicalHeaders =
        RequiredHeaders.Concat(OptionalHeaders).ToHashSet(StringComparer.OrdinalIgnoreCase);

    private static readonly Regex CandidateHeaderPattern =
        new(@"^[\p{L}\p{N}\s&/\-]+:?$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly HashSet<string> SmallTitleWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "a",
        "an",
        "and",
        "&",
        "for",
        "in",
        "of",
        "or",
        "the",
        "to",
        "with"
    };

    private static readonly HashSet<string> DegreeTerms = new(StringComparer.OrdinalIgnoreCase)
    {
        "ba",
        "bsc",
        "bs",
        "btech",
        "ma",
        "msc",
        "ms",
        "mba",
        "phd"
    };

    private static readonly HashSet<string> SectionSignalTerms = new(StringComparer.OrdinalIgnoreCase)
    {
        "activities",
        "achievements",
        "highlights",
        "impact",
        "interests",
        "publications",
        "talks",
        "toolkit"
    };

    public HeaderValidationResponse Validate(string resumeText)
    {
        var found = new Dictionary<string, HeaderMatch>(StringComparer.OrdinalIgnoreCase);
        var unclear = new List<string>();
        var seenUnclear = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var rawLine in SplitLines(resumeText))
        {
            var line = CleanHeaderCandidate(rawLine);

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var normalized = NormalizeHeader(line);

            if (TryMapKnownHeader(normalized, line, out var headerMatch))
            {
                AddBestHeaderMatch(found, headerMatch);
                continue;
            }

            if (IsLikelyUnclearHeader(line) && seenUnclear.Add(line))
            {
                unclear.Add(line);
            }
        }

        var rawHeaderScore = found.Values.Sum(GetHeaderPoints) -
            unclear.Count * UnclearHeaderPenalty;
        var headerQualityScore = Math.Clamp(rawHeaderScore, 0, 100);

        return new HeaderValidationResponse
        {
            HeaderQualityScore = headerQualityScore,
            HeadersFound = RequiredHeaders.Concat(OptionalHeaders)
                .Where(found.ContainsKey)
                .ToList(),
            HeadersMissing = RequiredHeaders.Concat(OptionalHeaders)
                .Where(header => !found.ContainsKey(header))
                .ToList(),
            UnclearHeaders = unclear,
            NonStandardHeaders = found.Values
                .Where(match => match.Quality == HeaderMatchQuality.WeakAlias)
                .Select(match => new NonStandardHeaderResponse
                {
                    HeaderFound = match.DetectedHeader,
                    MappedTo = match.CanonicalHeader,
                    RecommendedHeader = match.CanonicalHeader
                })
                .ToList(),
            StructureQuality = GetStructureQuality(headerQualityScore)
        };
    }

    private static IEnumerable<string> SplitLines(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        return text.Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n');
    }

    private static bool TryMapKnownHeader(
        string normalizedHeader,
        string detectedHeader,
        out HeaderMatch headerMatch)
    {
        if (CanonicalHeaders.TryGetValue(normalizedHeader, out var knownHeader))
        {
            headerMatch = new HeaderMatch(
                knownHeader,
                detectedHeader,
                HeaderMatchQuality.Standard);
            return true;
        }

        if (StrongHeaderAliases.TryGetValue(normalizedHeader, out var strongAliasedHeader))
        {
            headerMatch = new HeaderMatch(
                strongAliasedHeader,
                detectedHeader,
                HeaderMatchQuality.StrongAlias);
            return true;
        }

        if (WeakHeaderAliases.TryGetValue(normalizedHeader, out var weakAliasedHeader))
        {
            headerMatch = new HeaderMatch(
                weakAliasedHeader,
                detectedHeader,
                HeaderMatchQuality.WeakAlias);
            return true;
        }

        headerMatch = HeaderMatch.Empty;
        return false;
    }

    private static void AddBestHeaderMatch(
        Dictionary<string, HeaderMatch> found,
        HeaderMatch candidate)
    {
        if (!found.TryGetValue(candidate.CanonicalHeader, out var existing) ||
            candidate.Quality < existing.Quality)
        {
            found[candidate.CanonicalHeader] = candidate;
        }
    }

    private static int GetHeaderPoints(HeaderMatch match)
    {
        var isRequired = RequiredHeaders.Contains(match.CanonicalHeader, StringComparer.OrdinalIgnoreCase);

        return match.Quality == HeaderMatchQuality.WeakAlias
            ? isRequired ? WeakRequiredHeaderPoints : WeakOptionalHeaderPoints
            : isRequired ? RequiredHeaderPoints : OptionalHeaderPoints;
    }

    private static string CleanHeaderCandidate(string line)
    {
        return Regex.Replace(line.Trim(), @"\s+", " ")
            .Trim(' ', '\t', ':');
    }

    private static string NormalizeHeader(string header)
    {
        return header
            .Replace("and", "&", StringComparison.OrdinalIgnoreCase)
            .Trim();
    }

    private static bool IsLikelyUnclearHeader(string line)
    {
        if (line.Length is < 2 or > 64)
        {
            return false;
        }

        if (!CandidateHeaderPattern.IsMatch(line))
        {
            return false;
        }

        var words = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length is 0 or > 6)
        {
            return false;
        }

        if (DegreeTerms.Contains(words[0].Trim('.', ',', ':')))
        {
            return false;
        }

        return (IsAllCaps(line) || IsTitleLike(words)) &&
            words.Any(word => SectionSignalTerms.Contains(word.Trim('.', ',', ':')));
    }

    private static bool IsAllCaps(string line)
    {
        var letters = line.Where(char.IsLetter).ToArray();

        return letters.Length > 0 && letters.All(char.IsUpper);
    }

    private static bool IsTitleLike(string[] words)
    {
        return words.All(word =>
        {
            var trimmed = word.Trim('&', '/', '-');

            if (string.IsNullOrWhiteSpace(trimmed) || SmallTitleWords.Contains(trimmed))
            {
                return true;
            }

            return char.IsUpper(trimmed[0]);
        });
    }

    private static string GetStructureQuality(int score)
    {
        if (score >= 80)
        {
            return "strong";
        }

        return score >= 50 ? "adequate" : "weak";
    }

    private enum HeaderMatchQuality
    {
        Standard = 0,
        StrongAlias = 1,
        WeakAlias = 2
    }

    private sealed record HeaderMatch(
        string CanonicalHeader,
        string DetectedHeader,
        HeaderMatchQuality Quality)
    {
        public static HeaderMatch Empty { get; } = new(
            string.Empty,
            string.Empty,
            HeaderMatchQuality.WeakAlias);
    }
}
