using EShop.Infrastructure.Domain;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EShop.Core.Data.DbHandlers;

/// <summary>
/// This is a db entity's context consumed by a db handler to access the entity and the properties describing its state.
/// </summary>
public interface IHandleEntityContext
{
    public BaseEntity Entity { get; }
    public Type EntityType { get; }

    /// <summary>
    /// The state an entity was in right before the call to the actual SaveChanges() to persist the changes to the db.
    /// This is the state taken into account by the EF Framework when it thinks which operation to execute (add, remove, edit etc) against the database.
    ///
    /// The value is updated, if necessary, to match <see cref="EntityState"/> every time it comes out of a db handler before going into the next one.
    /// </summary>
    public EntityState InitialEntityState { get; set; }

    /// <summary>
    /// Represent the current, up-to-date state of an entity.
    /// You can change the state using the getter if you know what you're doing, and you're aware of the result of this action,
    /// though, most of the time it should be just read or not written.
    /// You can change the state of an entity to prevent it from getting removed from the db, for example. So when DetectChanges() is run
    /// </summary>
    public EntityState EntityState { get; set; }

    public EntityEntry Entry { get; }

    public bool HasStateChanged { get; }
}

public class HandleEntityContext : IHandleEntityContext
{
    public HandleEntityContext(EntityEntry entry)
    {
        Entry = entry;
        InitialEntityState = (EntityState)entry.State;
    }

    public BaseEntity Entity => (BaseEntity)Entry.Entity;

    private Type _entityType;
    public Type EntityType => _entityType ??= Entry.Entity.GetType();

    public EntityState EntityState
    {
        get => (EntityState)Entry.State;
        set => Entry.State = (Microsoft.EntityFrameworkCore.EntityState)value;
    }
    public EntityState InitialEntityState { get; set; }
    public EntityEntry Entry { get; }

    public bool HasStateChanged => EntityState != InitialEntityState;
}

public enum EntityState
{
    Detached,
    Unchanged,
    Deleted,
    Modified,
    Added,
}