namespace EShop.Core.Data.DbHandlers;

/// <summary>
/// This is a db entity's context consumed by a db handler to access the entity and the properties describing its state.
/// </summary>
public interface IHandleEntityContext
{
    public object Entity { get; set; }
    public DbState State { get; set; }
}

public enum EntityState
{
    Added,
    Modified,
    Deleted,
}