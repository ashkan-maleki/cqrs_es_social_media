using CQRS.Core.Domain;
using CQRS.Core.Events;
using CQRS.Core.Handlers;
using CQRS.Core.Infrastructure;
using CQRS.Core.Producers;
using Post.Command.Domain.Aggregates;

namespace Post.Command.Infrastructure.Handlers;

public class EventSourcingHandler  : IEventSourcingHandler<PostAggregate>
{
    private readonly IEventStore _eventStore;
    private readonly IEventProducer _eventProducer;

    public EventSourcingHandler(IEventStore eventStore, IEventProducer eventProducer)
    {
        _eventStore = eventStore;
        _eventProducer = eventProducer;
    }

    public async Task SaveAsync(AggregateRoot aggregate)
    {
        await _eventStore.SaveEventAsync(aggregate.Id,
            aggregate.GetUncommittedChanges(), 
            aggregate.Version);
        aggregate.MarkChangesAsCommitted();
    }

    public async Task<PostAggregate> GetByIdAsync(Guid aggregateId)
    {
        PostAggregate aggregate = new();
        List<BaseEvent> events = await _eventStore.GetEventAsync(aggregateId);

        if (events == null || !events.Any())
        {
            return aggregate;
        }
        
        aggregate.ReplayEvent(events);
        aggregate.Version = events
            .Select(x => x.Version)
            .Max()!.Value;

        return aggregate;
    }

    public async Task RepublishEventAsync()
    {
        string? topic = Environment.GetEnvironmentVariable("KAFKA_TOPIC");
        List<Guid> aggregateIds = await _eventStore.GetAggregateIdsAsync();
        if (aggregateIds == null || !aggregateIds.Any())
        {
            return;
        }

        foreach (Guid aggregateId in aggregateIds)
        {
            PostAggregate aggregate = await GetByIdAsync(aggregateId);
            
            if (aggregate == null || !aggregate.Active) continue;

            List<BaseEvent> events = await _eventStore.GetEventAsync(aggregateId);

            foreach (BaseEvent @event in events)
            {
                await _eventProducer.ProduceAsync(topic, @event);
            }
        }
    }
}