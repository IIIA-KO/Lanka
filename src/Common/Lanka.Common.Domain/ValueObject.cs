namespace Lanka.Common.Domain
{
    public abstract class ValueObject : IEquatable<ValueObject>
    {
        public abstract IEnumerable<object> GetAtomicValues();

        public override bool Equals(object? obj)
        {
            return obj is ValueObject other && this.ValuesAreEqual(other);
        }

        public bool Equals(ValueObject? other)
        {
            return other is not null && this.ValuesAreEqual(other);
        }

        public override int GetHashCode()
        {
            return this.GetAtomicValues()
                .Aggregate(0, HashCode.Combine);
        }

        private bool ValuesAreEqual(ValueObject other)
        {
            return this.GetAtomicValues()
                .SequenceEqual(other.GetAtomicValues());
        }
    }
}
