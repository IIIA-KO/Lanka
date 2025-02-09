using System.Data.Common;
using Dapper;
using Lanka.Common.Application.Data;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Users.Domain.Users;

namespace Lanka.Modules.Users.Application.Users.GetUser
{
    internal sealed class GetUserQueryHandler : IQueryHandler<GetUserQuery, UserResponse>
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public GetUserQueryHandler(IDbConnectionFactory dbConnectionFactory)
        {
            this._dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<Result<UserResponse>> Handle(GetUserQuery request, CancellationToken cancellationToken)
        {
            await using DbConnection connection = await this._dbConnectionFactory.OpenConnectionAsync();

            const string sql =
                $"""
                  SELECT
                      id AS {nameof(UserResponse.Id)},
                      email AS {nameof(UserResponse.Email)},
                      first_name AS {nameof(UserResponse.FirstName)},
                      last_name AS {nameof(UserResponse.LastName)},
                      birth_date AS {nameof(UserResponse.BirthDay)},
                  FROM users.users
                  WHERE id = @UserId
                  """;
            
            UserResponse? user = await connection.QuerySingleOrDefaultAsync<UserResponse>(sql, new { request.UserId });
            
            return user ?? Result.Failure<UserResponse>(UserErrors.NotFound(request.UserId));
        }
    }
}
