namespace Lanka.Common.Domain
{
    public abstract record  TypedEntityId
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
    }
}
