using System;
using System.Collections.Generic;
using System.Data.SqlTypes;

using Microsoft.SqlServer.Server;

namespace ZeidLab.ToolBox.EasyPersistence.EFCoreSqlClr;

public static class SqlClrFunctions
{
    [SqlFunction]
    public static SqlDouble FuzzySearch(SqlString searchTerm, SqlString comparedString)
    {
        if (searchTerm.IsNull || comparedString.IsNull)
            return new SqlDouble(0);

        string term = searchTerm.Value.Normalize();
        string compared = comparedString.Value.Normalize();

        // Quick exact match check (case-sensitive)
        if (compared.Contains(term))
            return new SqlDouble(1.0);

        // Quick case-insensitive check
        if (compared.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
            return new SqlDouble(0.8);

        return new SqlDouble(CalculateNGramSimilarity(term, compared));
    }

    private static double CalculateNGramSimilarity(string term, string compared)
    {
        // Convert to lowercase to ensure case-insensitive comparison
        term = term.ToLowerInvariant();
        compared = compared.ToLowerInvariant();
        
        double totalWeightedSimilarity = 0;
        double totalWeight = 0;

        // Limit n-gram size to improve performance while maintaining accuracy
        int maxNGramSize = Math.Min(term.Length, 4);
        
        for (int n = 1; n <= maxNGramSize; n++)
        {
            // Use arrays instead of dictionaries to avoid potential issues with SQL CLR
            var termNGrams = GenerateNGrams(term, n);
            var comparedNGrams = GenerateNGrams(compared, n);
            
            // Check if any n-grams were generated
            if (termNGrams.Length == 0 || comparedNGrams.Length == 0)
                continue;

            // Calculate intersection using simple array operations
            int intersection = CountIntersection(termNGrams, comparedNGrams);

            // Calculate Dice coefficient
            double diceCoefficient = (2.0 * intersection) / (termNGrams.Length + comparedNGrams.Length);

            // Weight by n-gram size relative to term length
            double weight = (double)n / Math.Max(1, maxNGramSize);
            totalWeightedSimilarity += diceCoefficient * weight;
            totalWeight += weight;
        }

        // Return normalized similarity score with a scaling factor
        return totalWeight > 0 ? (totalWeightedSimilarity / totalWeight) * 0.7 : 0;
    }
    
    private static string[] GenerateNGrams(string text, int n)
    {
        if (text.Length < n)
            return Array.Empty<string>();
            
        string[] result = new string[text.Length - n + 1];
        
        for (int i = 0; i <= text.Length - n; i++)
        {
            result[i] = text.Substring(i, n);
        }
        
        return result;
    }
      private static int CountIntersection(string[] array1, string[] array2)
    {
        int count = 0;
        
        // Use a simple algorithm that works well for small arrays
        // This is more efficient than using HashSet in SQL CLR with SAFE permission
        foreach (string item1 in array1)
        {
            foreach (string item2 in array2)
            {
                if (string.Equals(item1, item2, StringComparison.Ordinal))
                {
                    count++;
                    break;
                }
            }
        }
        
        return count;
    }
}