using System.Text;

namespace ZeidLab.ToolBox.EasyPersistence.EFCoreSqlClr.Test.Units;

internal static class NgramCalculatorExtensions
{
    internal static string[] Get3Grams(this string input)
    {
        // If the length is less than 3 or empty, return an empty array
        if (string.IsNullOrEmpty(input) || input.Length < 3)
            return [];

        // Normalize the input string
        var ns = input.Normalize().ToLowerInvariant();
        // Check the length of the input string
        // If the length is 3, return the input string as a single 3-gram
        if (ns.Length == 3)
            return [ns];

        if (ns.Length > 8)
        {
            return Enumerable.Range(0, ns.Length - 2)
                .Select(i => ns.Substring(i, 3))
                .ToHashSet().ToArray();
        }

        // Create a list to store the 3-grams
        var gramsList = new List<string>();
        for (int i = 0; i < ns.Length - 2; i++)
        {
            for (int j = 1; j < ns.Length - 1; j++)
            {
                if (j <= i) continue;
                for (int k = 2; k < ns.Length; k++)
                {
                    if (k <= j || j <= i) continue;
                    gramsList.Add($"{ns[i]}{ns[j]}{ns[k]}");
                }
            }
        }

        return gramsList.ToHashSet().ToArray();
    }

    internal static string Build3GramString(this string searchTerm)
    {
        var ns = searchTerm.Normalize().ToLowerInvariant();
        var hashSet = Enumerable.Range(0, ns.Length - 2)
            .Select(i => ns.Substring(i, 3))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        return string.Join(";", hashSet);
    }
}