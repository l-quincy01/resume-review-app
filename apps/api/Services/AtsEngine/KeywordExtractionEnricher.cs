using System.Text.RegularExpressions;
using ResumeReview.Api.Dtos.Responses;

namespace ResumeReview.Api.Services.AtsEngine;

public sealed class KeywordExtractionEnricher
{
    private const int MaxKeywordWords = 5;

    private static readonly HashSet<string> WeakGenericTerms = new(StringComparer.OrdinalIgnoreCase)
    {
        "team",
        "communication",
        "support",
        "business",
        "collaboration",
        "problem solving",
        "fast paced",
        "detail oriented",
        "stakeholder",
        "documentation"
    };

    private static readonly HashSet<string> AllowedFinalCategories = new(StringComparer.OrdinalIgnoreCase)
    {
        "technical_skill",
        "tool",
        "framework",
        "language",
        "methodology",
        "domain_keyword"
    };

    private static readonly string[] RejectedKeywordMarkers =
    [
        "ability to",
        "attitude",
        "can-do",
        "can do",
        "degree",
        "equivalent",
        "experience",
        "years"
    ];

    private static readonly HashSet<string> AllowedSlashTerms = new(StringComparer.OrdinalIgnoreCase)
    {
        "ci/cd",
        "ui/ux"
    };

    public KeywordExtractionResponse Enrich(
        KeywordExtractionResponse extraction,
        string jobDescription)
    {
        var filteredKeywords = extraction.Keywords
            .Where(IsUsefulAtsKeyword)
            .ToList();

        foreach (var keyword in filteredKeywords)
        {
            keyword.KeywordType = GetKeywordType(keyword.Keyword);
            keyword.Frequency = CountOccurrences(jobDescription, keyword.Keyword, keyword.Variations);

            var originalTier = keyword.Tier;
            if (ShouldBoost(keyword))
            {
                keyword.Tier = keyword.Tier switch
                {
                    1 => 0,
                    2 => 1,
                    3 => 2,
                    _ => keyword.Tier
                };
            }

            keyword.BoostApplied = keyword.Tier != originalTier;
        }

        extraction.Keywords = filteredKeywords;

        return extraction;
    }

    public static string GetKeywordType(string keyword)
    {
        return keyword.Contains(' ', StringComparison.Ordinal)
            ? "multi_word"
            : "single_word";
    }

    public static int CountOccurrences(
        string text,
        string keyword,
        IEnumerable<string>? variations = null)
    {
        var terms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            terms.Add(keyword.Trim());
        }

        foreach (var variation in variations ?? [])
        {
            if (!string.IsNullOrWhiteSpace(variation))
            {
                terms.Add(variation.Trim());
            }
        }

        return terms.Sum(term => Regex.Matches(
            text,
            $@"(?<![\p{{L}}\p{{N}}]){Regex.Escape(term)}(?![\p{{L}}\p{{N}}])",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant).Count);
    }

    private static bool ShouldBoost(KeywordExtractionItemResponse keyword)
    {
        return keyword.Frequency >= 2 &&
            keyword.Tier is 1 or 2 or 3 &&
            !WeakGenericTerms.Contains(keyword.Keyword) &&
            AllowedFinalCategories.Contains(keyword.Category);
    }

    private static bool IsUsefulAtsKeyword(KeywordExtractionItemResponse keyword)
    {
        var value = keyword.Keyword.Trim();

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (!AllowedFinalCategories.Contains(keyword.Category))
        {
            return false;
        }

        if (WeakGenericTerms.Contains(value))
        {
            return false;
        }

        if (CountWords(value) > MaxKeywordWords)
        {
            return false;
        }

        if (RejectedKeywordMarkers.Any(marker =>
            value.Contains(marker, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        if (value.Contains('/', StringComparison.Ordinal) &&
            !AllowedSlashTerms.Contains(value))
        {
            return false;
        }

        return true;
    }

    private static int CountWords(string value)
    {
        return Regex.Matches(value, @"[\p{L}\p{N}]+", RegexOptions.CultureInvariant).Count;
    }
}
