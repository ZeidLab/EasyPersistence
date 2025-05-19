using System;
using System.Data.SqlTypes;
using System.Linq;

using Microsoft.SqlServer.Server;

namespace ZeidLab.ToolBox.EasyPersistence.EFCoreSqlClr;

public static class SqlClrFunctions
{
    [SqlFunction]
    public static SqlDouble FuzzySearch(SqlString searchTerm, SqlString comparedString)
    {
        if (string.IsNullOrWhiteSpace(comparedString.Value)
            || string.IsNullOrWhiteSpace(searchTerm.Value)
           )
            return new SqlDouble(0);

        var grams = searchTerm.Value
            .Normalize()
            .ToLowerInvariant()
            .Split([';'], StringSplitOptions.RemoveEmptyEntries);
        // If the search term is empty or null, return 0
        if (grams.Length < 2)
            return new SqlDouble(0);

        if (!int.TryParse(grams[0].Trim(), out int originalTermLength) || originalTermLength < 3)
        {
            // If the first part of the search term is not a valid integer or less than 3, return 0
            return new SqlDouble(0);
        }
        

        // Normalize the strings to ensure consistent comparison
        string compared = comparedString.Value
            .Normalize()
            .ToLowerInvariant();


        // after normalization if the term length is grater than compared length should return 0
        if (compared.Length < 3)
            return new SqlDouble(0);

        // Find all occurrences of term in compared (case-insensitive)
        //int matchedChars = 0;

        // foreach (string gram in grams)
        // {
        //     for (int i = 0; i < compared.Length -2; i++)
        //     {
        //         var j = i + 1;
        //         var k = i + 2;
        //         if (gram[0] == compared[i] && gram[1] == compared[j] && gram[2] == compared[k])
        //         {
        //             matchedChars++;
        //         }
        //     }
        // }

        var comparedGrams = Enumerable.Range(0, compared.Length - 2)
            .Select(i => compared.Substring(i, 3))
            .ToHashSet();

        int hits = grams.Skip(1)
            .Count(gram => comparedGrams.Contains(gram));

        var score = DiceCoefficientCalculation(hits, originalTermLength, comparedGrams.Count);

        return new SqlDouble(score);
    }

    private static double DiceCoefficientCalculation(int hits, int termGramsLength, int compGramsLength)
    {
        // Compute Dice similarity: 2 * |intersection| / (|set1| + |set2|)
        return (2.0 * hits) / (termGramsLength + compGramsLength);
    }
}