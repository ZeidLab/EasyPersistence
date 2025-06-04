using System.Data.SqlTypes;
using System.Text;

using FluentAssertions;

namespace ZeidLab.ToolBox.EasyPersistence.EFCoreSqlClr.Test.Units
{
    public class SqlClrFunctionsTests
    {
        [Fact]
        public void FuzzySearch_WithEmptyNullOrShortInputs_ShouldReturnZero()
        {
            // Arrange
            SqlString emptyString = new SqlString(string.Empty);
            SqlString whitespaceString = new SqlString("   ");
            SqlString nonEmptyString = new SqlString("test");
            SqlString tooShort = new SqlString("ab"); // Less than 3 chars

            // Act & Assert
            SqlClrFunctions.FuzzySearch(emptyString, nonEmptyString).Value.Should().Be(0);
            SqlClrFunctions.FuzzySearch(nonEmptyString, emptyString).Value.Should().Be(0);
            SqlClrFunctions.FuzzySearch(whitespaceString, nonEmptyString).Value.Should().Be(0);
            SqlClrFunctions.FuzzySearch(nonEmptyString, tooShort).Value.Should().Be(0);
        }

        [Theory]
        [InlineData("test", "TEST", 1.0)] // Case difference
        [InlineData("test", "TeSt", 1.0)] // Mixed case
        [InlineData("test-123", "test-123!", 0.8)] // Special chars
        [InlineData("test:abc", "test;abc", 0.5)] // Different special chars
        public void FuzzySearch_WithCaseAndSpecialChars_ShouldMatchAppropriately(string search, string compared, double expectedScore)
        {
            // Arrange
            SqlString searchTerm = new SqlString(search.Build3GramString());
            SqlString comparedTerm = new SqlString(compared);

            // Act
            var result = SqlClrFunctions.FuzzySearch(searchTerm, comparedTerm).Value;

            // Assert
            result.Should().BeGreaterThanOrEqualTo(expectedScore);
        }

        [Theory]
        [InlineData("testing", "testign", 0.4)] // Transposition
        [InlineData("testing", "testng", 0.4)] // Missing character
        [InlineData("testing", "testting", 0.4)] // Extra character
        [InlineData("iPhone 13 Pro Max", "iPhnoe 13 Pro", 0.4)] // Real-world typo
        [InlineData("target", "aaaaaaaatargetbbbbbbb", 0.9)] // Target within long string
        public void FuzzySearch_WithTyposAndPartialMatches_ShouldReturnReasonableScores(string correct, string withTypo, double minExpectedScore)
        {
            // Arrange
            SqlString correctTerm = new SqlString(correct.Build3GramString());
            SqlString typoTerm = new SqlString(withTypo);

            // Act
            var result = SqlClrFunctions.FuzzySearch(correctTerm, typoTerm).Value;

            // Assert
            result.Should().BeGreaterThan(minExpectedScore);
        }

        [Fact]
        public void FuzzySearch_PositioningAndMultipleOccurrences_ShouldScoreAppropriately()
        {
            // Arrange - Test positions
            SqlString searchTerm = new SqlString("test".Build3GramString());
            SqlString prefixMatch = new SqlString("test result");
            SqlString middleMatch = new SqlString("a test result");
            SqlString suffixMatch = new SqlString("result test");
            
            // Arrange - Test multiple occurrences
            SqlString multipleTerm = new SqlString("testing".Build3GramString());
            SqlString singleOccurrence = new SqlString("testing");
            SqlString multipleOccurrences = new SqlString("testingtesting");

            // Act - Positioning tests
            var prefixResult = SqlClrFunctions.FuzzySearch(searchTerm, prefixMatch).Value;
            var middleResult = SqlClrFunctions.FuzzySearch(searchTerm, middleMatch).Value;
            var suffixResult = SqlClrFunctions.FuzzySearch(searchTerm, suffixMatch).Value;
            
            // Act - Multiple occurrences tests
            var singleResult = SqlClrFunctions.FuzzySearch(multipleTerm, singleOccurrence).Value;
            var multipleResult = SqlClrFunctions.FuzzySearch(multipleTerm, multipleOccurrences).Value;

            // Assert - Positioning
            prefixResult.Should().BeGreaterThan(0.9);
            middleResult.Should().BeGreaterThan(0.9);
            suffixResult.Should().BeGreaterThan(0.9);
            
            // Assert - Multiple occurrences
            singleResult.Should().Be(1.0);
            multipleResult.Should().BeGreaterThan(0.8);
        }

        [Fact]
        public void FuzzySearch_WithInternationalAndUnicodeText_ShouldHandleNormalization()
        {
            // Arrange - Test with different scripts
            SqlString latinTerm = new SqlString("hello world".Build3GramString());
            SqlString cyrillicTerm = new SqlString("привет мир".Build3GramString()); // Russian "hello world"
            SqlString cjkTerm = new SqlString("你好世界".Build3GramString()); // Chinese "hello world"
            
            SqlString latin = new SqlString("hello world");
            SqlString cyrillic = new SqlString("привет мир"); 
            SqlString cjk = new SqlString("你好世界");

            // Arrange - Test normalization
            SqlString precomposedTerm = new SqlString("café".Build3GramString()); // é is a single code point
            SqlString decomposedTerm = new SqlString("cafe\u0301".Build3GramString()); // e + combining accent

            SqlString precomposed = new SqlString("café");
            SqlString decomposed = new SqlString("cafe\u0301");

            // Act & Assert - Match within same scripts
            SqlClrFunctions.FuzzySearch(latinTerm, latin).Value.Should().Be(1.0);
            SqlClrFunctions.FuzzySearch(cyrillicTerm, cyrillic).Value.Should().Be(1.0);
            SqlClrFunctions.FuzzySearch(cjkTerm, cjk).Value.Should().Be(1.0);

            // Different scripts should have low similarity
            SqlClrFunctions.FuzzySearch(latinTerm, cyrillic).Value.Should().BeLessThan(0.1);

            // Normalization tests
            SqlClrFunctions.FuzzySearch(precomposedTerm, decomposed).Value.Should().Be(1.0);
            SqlClrFunctions.FuzzySearch(decomposedTerm, precomposed).Value.Should().Be(1.0);
        }
    }
}

