namespace EShop.Infrastructure.Domain;

public abstract class BaseEntity : IEquatable<BaseEntity>
{


    public int Id { get; set; }

    
    //INFO: GetHashCode() should 'match' the Equals()'s logic to work properly in hash tables. 
    public override int GetHashCode()
    {
        if (!HasRealId())
        {
            //INFO: In this case, we shouldn't treat the object as if it represented an entity in the database. 
            return base.GetHashCode();
        }

        unchecked
        { 
            var hashCode = GetType()
                .GetHashCode();
            return (hashCode * 31) ^ Id.GetHashCode();
        }
    }

    public override bool Equals(object obj)
    {
        //INFO: To make sure that both objects are BaseEntity. If not, the first check will return false.
        return Equals(obj as BaseEntity);
    }

    //INFO: In the database an entity is defined by its primary key. We want two objects to be semantically equal if they refer the same entity in the database.
    //This is what consumers of entity types would expect because in DDD, two objects refer the same entity if they have the same ID.
    public virtual bool Equals(BaseEntity other)
    {
        if (other == null)
            return false;

        //Equality has to be reflexive (an object equals itself).
        if (ReferenceEquals(this, other))
            return true;

        if (HasSameRealId(other) && IsSameType(other))
            return true;

        return false;
    }

    public static bool operator ==(BaseEntity x, BaseEntity y)
    {
        //INFO: if both are null - return true otherwise if they are equal - return true, if not - false.
        if (Equals(x, null))
        {
            return Equals(y, null);
        }

        return x.Equals(y);
    }

    public static bool operator !=(BaseEntity x, BaseEntity y)
    {
        return !(x == y);
    }


    private bool HasSameRealId(BaseEntity other)
        => HasRealId() && other.HasRealId() && Id == other.Id;

    private bool IsSameType(BaseEntity other)
        => GetType() == other.GetType();

    //INFO: If this instance's ID is zero, it means that this instance doesn't represent any actual entity that is sitting in the database,
    // which means that we are going to treat as any other object in terms of comparison. If the instance's id is anything but zero,
    // that instance does represent a particular record in the database,
    // and so we are going to compare it using our semantic approach.
    public virtual bool HasRealId()
    {
        return Id != 0;
    }

    public virtual string GetEntityName() => GetType()
        .Name;
}