using System.Data;
using Dapper;

namespace Lanka.Common.Infrastructure.Data;

internal sealed class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override DateOnly Parse(object value)
    {
        return value switch
        {
            DateOnly d => d,
            DateTime dt => DateOnly.FromDateTime(dt),
            _ => throw new DataException($"Cannot convert {value.GetType()} to DateOnly")
        };
    }


    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.Value = DbType.Date;
        parameter.Value = value;
    }
}
