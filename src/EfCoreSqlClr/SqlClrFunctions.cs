using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

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

        // Split the search term into 3-gram array using ';' as a delimiter
        // This should be already a normalized lowercased 3-gram string
        var grams = searchTerm.Value
            .Split([';'], StringSplitOptions.RemoveEmptyEntries);

        // If the search term is empty or null, return 0
        if (grams.Length == 0)
            return new SqlDouble(0);


        // Normalize the strings to ensure consistent comparison
        string compared = comparedString.Value.Normalize();


        // after normalization if the term length is less than 3, return 0
        if (compared.Length < 3)
            return new SqlDouble(0);

        // generate 3-grams from the compared string
        var comparedGrams = Enumerable.Range(0, compared.Length - 2)
            .Select(i => compared.Substring(i, 3))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // count hits
        var hits = grams.Count(x => comparedGrams.Contains(x));
        
        
        // If no 3-grams are found, return 0
        if (hits == 0)
            return new SqlDouble(0);

        // Calculate the score based on the number of hits and coverage
        var score = ScoreCalculation(hits, grams.Length, comparedGrams.Count);

        return new SqlDouble(score);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double ScoreCalculation(int hits, int gramsLength, int comparedGramsLength)
    {
        // Calculate the coverage score of search term as a percentage
        // The maximum coverage is between 0 and 1;
        var coverageScore = (double)hits / gramsLength;

        // Calculate the similarity score based on the number of hits
        // The maximum similarity score is between 0 and 1;
        var similarityScore = DiceCoefficientCalculation(hits, gramsLength, comparedGramsLength);

        // Calculate the final score as a weighted average of coverage and similarity
        // The weights are 10 for coverage and 1 for similarity
        // The final score is between 0 and 1;
        return ((coverageScore * 10) + similarityScore) / 11;
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double DiceCoefficientCalculation(int hits, int termGramsLength, int compGramsLength)
    {
        // Compute Dice similarity: 2 * |intersection| / (|set1| + |set2|)
        return (2.0 * hits) / (termGramsLength + compGramsLength);
    }
}