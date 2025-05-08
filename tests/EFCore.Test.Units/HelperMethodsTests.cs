using System.Linq.Expressions;

using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Units.Models;

namespace ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Units;

public class HelperMethodsTests
{
    [Fact]
    public void WhereIf_TrueCondition_ReturnsFilteredQueryable()
    {
        // Arrange
        var data = new List<TestEntityBase>
        {
            new TestEntityBase(1, "Test1"),
            new TestEntityBase(2, "Test2"),
            new TestEntityBase(3, "Test3")
        }.AsQueryable();

        const bool condition = true;
        Expression<Func<TestEntityBase, bool>> predicate = e => e.Name.Contains("Test");

        // Act
        var result = data.WhereIf(condition, predicate);

        // Assert
        Assert.Equal(3, result.Count());
    }

    [Fact]
    public void WhereIf_FalseCondition_ReturnsUnfilteredQueryable()
    {
        // Arrange
        var data = new List<TestEntityBase>
        {
            new TestEntityBase(1, "Test1"),
            new TestEntityBase(2, "Test2"),
            new TestEntityBase(3, "Test3")
        }.AsQueryable();

        const bool condition = false;
        Expression<Func<TestEntityBase, bool>> predicate = e => e.Name.Contains("Test");

        // Act
        var result = data.WhereIf(condition, predicate);

        // Assert
        Assert.Equal(3, result.Count());
    }
}