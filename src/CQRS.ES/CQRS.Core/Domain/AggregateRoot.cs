using System.Reflection;
using CQRS.Core.Events;

namespace CQRS.Core.Domain;

public abstract class AggregateRoot
{
    public Guid Id { get; protected set; }

    private readonly List<BaseEvent> _changes = new();

    public int Version { get; set; } = -1;

    public IEnumerable<BaseEvent> GetUncommittedChanges() => _changes;
    public void MarkChangesAsCommitted() => _changes.Clear();

    private void ApplyChange(BaseEvent @event, bool isNew)
    {
        MethodInfo? method = this.GetType().GetMethod("Apply", 
            new Type[] {@event.GetType()});

        if (method == null)
        {
            throw new ArgumentNullException(@event.GetType().Name,
                "The apply method was not found in " +
                $"the aggregate for {@event.GetType().Name}");
        }

        method.Invoke(this, new object?[] {@event});

        if (isNew)
        {
            _changes.Add(@event);
        }
    }

    protected void RaiseEvent(BaseEvent @event) => ApplyChange(@event, true);

    public void ReplayEvent(IEnumerable<BaseEvent> events)
    {
        foreach (BaseEvent @event in events)
        {
            ApplyChange(@event, false);
        }
    }
}