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

        if (compared.Contains(term))
            return new SqlDouble(1.0);

        if (compared.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
            return new SqlDouble(0.8);

        return new SqlDouble(CalculateNGramSimilarity(term, compared));
    }

    private static double CalculateNGramSimilarity(string term, string compared)
    {
        term = term.ToLowerInvariant();
        compared = compared.ToLowerInvariant();

        double totalWeightedSimilarity = 0;
        double totalWeight = 0;

        for (int n = 1; n <= term.Length; n++)
        {
            var termNGrams = GenerateNGrams(term, n);
            var comparedNGrams = GenerateNGrams(compared, n);

            if (termNGrams.Count == 0 || comparedNGrams.Count == 0)
                continue;

            int intersection = 0;
            foreach (var ngram in termNGrams)
            {
                if (comparedNGrams.Contains(ngram))
                    intersection++;
            }

            double diceCoefficient = (2.0 * intersection) / (termNGrams.Count + comparedNGrams.Count);
            double weight = (double)n / term.Length;
            totalWeightedSimilarity += diceCoefficient * weight;
            totalWeight += weight;
        }

        return totalWeight > 0 ? (totalWeightedSimilarity / totalWeight) * 0.7 : 0;
    }

    private static HashSet<CharSpan> GenerateNGrams(string text, int n)
    {
        var nGrams = new HashSet<CharSpan>();

        if (text.Length < n)
            return nGrams;

        for (int i = 0; i <= text.Length - n; i++)
        {
            nGrams.Add(new CharSpan(text, i, n));
        }

        return nGrams;
    }
}

internal struct CharSpan : IEquatable<CharSpan>
{
    private readonly string _source;
    private readonly int _start;
    private readonly int _length;
    private readonly int _hashCode;

    public CharSpan(string source, int start, int length)
    {
        _source = source;
        _start = start;
        _length = length;

        unchecked
        {
            int hash = 17;
            for (int i = 0; i < _length; i++)
            {
                hash = hash * 31 + _source[_start + i].GetHashCode();
            }

            _hashCode = hash;
        }
    }

    public bool Equals(CharSpan other)
    {
        if (_length != other._length) return false;

        for (int i = 0; i < _length; i++)
        {
            if (_source[_start + i] != other._source[other._start + i])
                return false;
        }

        return true;
    }

    public override bool Equals(object obj) => obj is CharSpan other && Equals(other);

    public override int GetHashCode() => _hashCode;
}