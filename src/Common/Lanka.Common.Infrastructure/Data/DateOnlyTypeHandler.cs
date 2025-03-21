using System.Data;
using Dapper;

namespace Lanka.Common.Infrastructure.Data;

internal sealed class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override DateOnly Parse(object value) =>
        DateOnly.FromDateTime((DateTime)value);
        
    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.Value = DbType.Date;
        parameter.Value = value;
    }
}
