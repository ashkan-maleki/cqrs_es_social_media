using CQRS.Core.Domain;
using CQRS.Core.Events;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Post.Command.Infrastructure.Config;

namespace Post.Command.Infrastructure.Repositories;

public class EventStoreRepository : IEventStoreRepository
{
    private readonly IMongoCollection<EventModel> _eventStoreCollection;

    public EventStoreRepository(IOptions<MongoDbConfig> options)
    {
        MongoDbConfig config = options.Value;
        MongoClient mongoClient = new(config.ConnectionString);
        IMongoDatabase? mongoDatabase = mongoClient.GetDatabase(config.Database);
        _eventStoreCollection = mongoDatabase.GetCollection<EventModel>(config.Collection);
    }

    public async Task SaveAsync(EventModel @event) =>
        await _eventStoreCollection.InsertOneAsync(@event).ConfigureAwait(false);

    public async Task<List<EventModel>> FindByAggregateId(Guid aggregateId) =>
        await _eventStoreCollection
            .Find(x => x.AggregateIdentifier == aggregateId)
            .ToListAsync().ConfigureAwait(false);

    public async Task<List<EventModel>> FindAllAsync() =>
        await _eventStoreCollection.Find(_ => true).ToListAsync().ConfigureAwait(false);
}