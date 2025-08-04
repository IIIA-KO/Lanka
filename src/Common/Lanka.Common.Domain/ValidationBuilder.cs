namespace Lanka.Common.Domain;

public class ValidationBuilder
{
    private readonly List<Result> _results = [];

    public ValidationBuilder Add(Result result)
    {
        this._results.Add(result);
        return this;
    }

    public ValidationBuilder Add<T>(Result<T> result)
    {
        this._results.Add(result);
        return this;
    }

    public Result<T> Build<T>(Func<T> valueFactory)
    {
        Result[] failures = this._results.Where(r => r.IsFailure).ToArray();

        if (failures.Any())
        {
            return Result.Failure<T>(ValidationError.FromResults(failures));
        }
        
        return valueFactory();
    }
}
