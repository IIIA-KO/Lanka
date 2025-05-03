using System.Data.Common;
using Dapper;
using Lanka.Common.Application.Data;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Domain.Bloggers;

namespace Lanka.Modules.Campaigns.Application.Bloggers.GetBlogger;

internal sealed class GetBloggerQueryHandler
    : IQueryHandler<GetBloggerQuery, BloggerResponse>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public GetBloggerQueryHandler(IDbConnectionFactory dbConnectionFactory)
    {
        this._dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<Result<BloggerResponse>> Handle(GetBloggerQuery request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await this._dbConnectionFactory.OpenConnectionAsync();
        
        const string sql =
            $"""
             SELECT
                 id AS {nameof(BloggerResponse.Id)},
                 first_name AS {nameof(BloggerResponse.FirstName)},
                 last_name AS {nameof(BloggerResponse.LastName)},
                 email AS {nameof(BloggerResponse.Email)},
                 birth_date AS {nameof(BloggerResponse.BirthDate)},
                 bio AS {nameof(BloggerResponse.Bio)}
             FROM campaigns.bloggers
             WHERE id = @BloggerId
             """;
        
        BloggerResponse? blogger = await connection.QuerySingleOrDefaultAsync<BloggerResponse>(
            sql,
            new { request.BloggerId }
        );
        
        return blogger ?? Result.Failure<BloggerResponse>(BloggerErrors.NotFound);
    }
}
