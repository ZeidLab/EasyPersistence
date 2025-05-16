using System.Data.SqlTypes;

using FluentAssertions;

namespace ZeidLab.ToolBox.EasyPersistence.EFCoreSqlClr.Test.Units
{
    public class SqlClrFunctionsTests
    {
        [Fact]
        public void FuzzySearch_WithNullInputs_ShouldReturnZero()
        {
            // Arrange
            SqlString nullString = SqlString.Null;
            SqlString validString = new SqlString("test");

            // Act & Assert
            SqlClrFunctions.FuzzySearch(nullString, validString).Value.Should().Be(0);
            SqlClrFunctions.FuzzySearch(validString, nullString).Value.Should().Be(0);
            SqlClrFunctions.FuzzySearch(nullString, nullString).Value.Should().Be(0);
        }

        [Fact]
        public void FuzzySearch_WithExactMatch_ShouldReturnOne()
        {
            // Arrange
            SqlString searchTerm = new SqlString("test");
            SqlString exactMatch = new SqlString("test");
            SqlString containsMatch = new SqlString("this is a test string");

            // Act & Assert
            SqlClrFunctions.FuzzySearch(searchTerm, exactMatch).Value.Should().Be(1.0);
            SqlClrFunctions.FuzzySearch(searchTerm, containsMatch).Value.Should().Be(1.0);
        }
        
        [Fact]
        public void FuzzySearch_WithPartialPrefixMatch_ShouldReturnProportionalScore()
        {
            // Arrange
            SqlString searchTerm = new SqlString("testing");
            SqlString partialPrefix = new SqlString("test");

            // Act
            var result = SqlClrFunctions.FuzzySearch(searchTerm, partialPrefix).Value;

            // Assert
            // 4 characters match out of 7, so 4/7 * 0.7 ≈ 0.4
            result.Should().BeApproximately(4.0 / 7.0 * 0.7, 0.03);
        }

        [Fact]
        public void FuzzySearch_WithNoMatch_ShouldReturnZero()
        {
            // Arrange
            SqlString searchTerm = new SqlString("apple");
            SqlString noMatch = new SqlString("bhnhnh");

            // Act
            var result = SqlClrFunctions.FuzzySearch(searchTerm, noMatch).Value;

            // Assert
            result.Should().Be(0);
        }
    }
}