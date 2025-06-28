// ReSharper disable once CheckNamespace
namespace ZeidLab.ToolBox.EasyPersistence.EFCore;

// Provides n-gram calculation utilities for fuzzy search operations.
internal static class NgramCalculatorExtensions
{
    /// <summary>
    /// Builds a semicolon-separated string of unique 3-grams from the normalized input.
    /// </summary>
    /// <param name="searchTerm">The input string to process.</param>
    /// <returns>A semicolon-separated string of unique 3-grams.</returns>
    internal static string Build3GramString(this string searchTerm)
    {
        var ns = searchTerm.Normalize().ToLowerInvariant();
        var hashSet = Enumerable.Range(0, ns.Length - 2)
            .Select(i => ns.Substring(i, 3))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        return string.Join(';', hashSet);
    }
}