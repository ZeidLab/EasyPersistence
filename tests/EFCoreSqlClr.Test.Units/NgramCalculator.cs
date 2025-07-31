namespace ZeidLab.ToolBox.EasyPersistence.EFCoreSqlClr.Test.Units;

internal static class NgramCalculatorExtensions
{
    internal static string Build3GramString(this string searchTerm)
    {
        var ns = searchTerm.Normalize().ToLowerInvariant();
        var hashSet = Enumerable.Range(0, ns.Length - 2)
            .Select(i => ns.Substring(i, 3))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        return string.Join(";", hashSet);
    }
}