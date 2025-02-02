namespace Lanka.Common.Domain
{
    public sealed record ValidationError : Error
    {
        public ValidationError(Error[] errors)
            : base(
                "General.Validation",
                "One or more validation errors occurred",
                ErrorType.Validation
            )
        {
            this.Errors = errors;
        }

        public IReadOnlyList<Error> Errors { get; }

        public static ValidationError FromResults(IEnumerable<Result> results)
        {
            return new ValidationError(
                results.Where(r => r.IsFailure).Select(r => r.Error).ToArray()
            );
        }
    }
}
