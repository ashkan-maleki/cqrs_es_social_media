using CQRS.Core.Domain;
using CQRS.Core.Events;
using CQRS.Core.Handlers;
using CQRS.Core.Infrastructure;
using Post.Command.Domain.Aggregates;

namespace Post.Command.Infrastructure.Handlers;

public class EventSourcingHandler  : IEventSourcingHandler<PostAggregate>
{
    private readonly IEventStore _eventStore;

    public EventSourcingHandler(IEventStore eventStore)
    {
        _eventStore = eventStore;
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
}