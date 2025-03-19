namespace Lanka.Common.Domain
{
#pragma warning disable CA1716
    public record Error
#pragma warning restore CA1716
    {
        public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

        public static readonly Error NullValue = new(
            "General.Null",
            "Null value was provided",
            ErrorType.Failure
        );

        public static Error NotAuthorized => Unauthorized(
            "General.NotAuthorized",
            "User is not authorized to perform this action"
        );
        
        public Error(string code, string description, ErrorType type)
        {
            this.Code = code;
            this.Description = description;
            this.Type = type;
        }

        public string Code { get; }

        public string Description { get; }

        public ErrorType Type { get; }

        public static Error Failure(string code, string description)
        {
            return new Error(code, description, ErrorType.Failure);
        }

        public static Error NotFound(string code, string description)
        {
            return new Error(code, description, ErrorType.NotFound);
        }

        public static Error Problem(string code, string description)
        {
            return new Error(code, description, ErrorType.Problem);
        }

        public static Error Conflict(string code, string description)
        {
            return new Error(code, description, ErrorType.Conflict);
        }

        public static Error Validation(string code, string description)
        {
            return new Error(code, description, ErrorType.Validation);
        }

        public static Error Unauthorized(string code, string description)
        {
            return new Error(code, description, ErrorType.Unauthorized);
        }
    }
}
