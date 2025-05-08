namespace ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Units.Models;

internal sealed class TestEntityBase : EntityBase<int>
{
    public TestEntityBase(int id, string name) : base(id)
    {
        Name = name;
    }

    public string Name { get; private set; }
}