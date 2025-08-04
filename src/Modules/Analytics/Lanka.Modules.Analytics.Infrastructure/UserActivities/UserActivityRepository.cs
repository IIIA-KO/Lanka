using Lanka.Modules.Analytics.Domain.UserActivities;
using Lanka.Modules.Analytics.Infrastructure.Instagram;
using MongoDB.Driver;

namespace Lanka.Modules.Analytics.Infrastructure.UserActivities;

internal sealed class UserActivityRepository : IUserActivityRepository
{
    private readonly IMongoCollection<UserActivity> _collection;

    public UserActivityRepository(IMongoClient mongoClient)
    {
        IMongoDatabase database = mongoClient.GetDatabase(DocumentDbSettings.Database);
        this._collection = database.GetCollection<UserActivity>(DocumentDbSettings.UserActivity);
    }

    public async Task<UserActivity?> GetAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        FilterDefinition<UserActivity> filter = Builders<UserActivity>
            .Filter.Eq(x => x.UserId, userId);

        UserActivity? userActivity = await this._collection.Find(filter).SingleOrDefaultAsync(cancellationToken);

        return userActivity;
    }

    public async Task InsertAsync(UserActivity userActivity, CancellationToken cancellationToken = default)
    {
        await this._collection.InsertOneAsync(userActivity, cancellationToken: cancellationToken);
    }

    public async Task ReplaceAsync(UserActivity userActivity, CancellationToken cancellationToken = default)
    {
        FilterDefinition<UserActivity> filter = Builders<UserActivity>
            .Filter
            .Eq(x => x.UserId, userActivity.UserId);

        var options = new ReplaceOptions { IsUpsert = true };
        
        await this._collection.ReplaceOneAsync(filter, userActivity, options, cancellationToken);
    }
}
