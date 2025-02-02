namespace Lanka.Common.Domain
{
    public abstract class TypedEntityId : IEquatable<TypedEntityId>
    {
        public Guid Value { get; }

        protected TypedEntityId(Guid value)
        {
            if (value == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(value), $"The value {value} cannot be empty.");
            }
            
            this.Value = value;
        }
        
        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }
        
        public override bool Equals(object? obj)
        {
            if (obj is null)
            {
                return false;
            }

            return obj is TypedEntityId other && this.Equals(other);
        }
        
        public bool Equals(TypedEntityId? other)
        {
            return this.Value == other?.Value;
        }
        
        public static bool operator ==(TypedEntityId obj1, TypedEntityId obj2)
        {
            if (object.Equals(obj1, null))
            {
                if (object.Equals(obj2, null))
                {
                    return true;
                }

                return false;
            }

            return obj1.Equals(obj2);
        }
        
        public static bool operator !=(TypedEntityId x, TypedEntityId y)
        {
            return !(x == y);
        }
    }
}
