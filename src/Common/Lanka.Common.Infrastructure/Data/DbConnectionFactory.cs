using System.Data.Common;
using Lanka.Common.Application.Data;
using Npgsql;

namespace Lanka.Common.Infrastructure.Data
{
    internal sealed class DbConnectionFactory(NpgsqlDataSource dataSource) : IDbConnectionFactory
    {
        public async ValueTask<DbConnection> OpenConnectionAsync()
        {
            return await dataSource.OpenConnectionAsync();
        }
    }
}
