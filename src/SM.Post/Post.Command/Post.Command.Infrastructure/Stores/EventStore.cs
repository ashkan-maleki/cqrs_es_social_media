using CQRS.Core.Domain;
using CQRS.Core.Events;
using CQRS.Core.Exceptions;
using CQRS.Core.Infrastructure;
using Post.Command.Domain.Aggregates;

namespace Post.Command.Infrastructure.Stores;

public class EventStore : IEventStore
{
    private readonly IEventStoreRepository _eventStoreRepository;

    public EventStore(IEventStoreRepository eventStoreRepository)
    {
        _eventStoreRepository = eventStoreRepository;
    }

    public async Task SaveEventAsync(Guid aggregateId,
        IEnumerable<BaseEvent> events,
        int expectedVersion)
    {
        List<EventModel>? eventStream = await _eventStoreRepository
            .FindByAggregateId(aggregateId);

        if (expectedVersion != -1 && eventStream[^1].Version != null)
        {
            throw new ConcurrencyException();
        }

        int version = expectedVersion;

        foreach (BaseEvent @event in events)
        {
            version++;
            @event.Version = version;
            string eventType = @event.GetType().Name;
            EventModel eventModel = new()
            {
                TimeStamp = DateTime.Now,
                AggregateIdentifier = aggregateId,
                AggregateType = nameof(PostAggregate),
                Version = version,
                EventType = eventType,
                EventData = @event
            };

            await _eventStoreRepository.SaveAsync(eventModel);
        }
    }

    public async Task<List<BaseEvent>> GetEventAsync(Guid aggregateId)
    {
        List<EventModel>? eventStream = await _eventStoreRepository
            .FindByAggregateId(aggregateId);

        if (eventStream == null || !eventStream.Any())
        {
            throw new AggregateNotFoundException("Incorrect post ID provided");
        }

        return eventStream!.OrderBy(x => x.Version)
            .Select(x => x.EventData).ToList()!;
    }
}