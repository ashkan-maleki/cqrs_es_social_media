using CQRS.Core.Domain;
using MongoDB.Bson;
using Post.Common.Events;

namespace Post.Command.Domain.Aggregates;

public class PostAggregate : AggregateRoot
{
    private bool _active;
    private string? _author;
    private readonly Dictionary<Guid, Tuple<string, string>> _comments = new();

    public bool Active
    {
        get => _active;
        set => _active = value;
    }

    public PostAggregate()
    {
    }

    public PostAggregate(Guid id, string author, string message)
        => RaiseEvent(new PostCreatedEvent
        {
            Id = id,
            Author = author,
            Message = message,
            DatePosted = DateTime.Now
        });

    public void Apply(PostCreatedEvent @event)
    {
        Id = @event.Id;
        _active = true;
        _author = @event.Author;
    }


    private void ThrowInvalidOperationExceptionIfNotActive()
    {
        if (!_active)
        {
            string errorMessage = "You cannot edit the message of an inactive post!";
            throw new InvalidOperationException(errorMessage);
        }
    }


    private void ThrowInvalidOperationExceptionIfStringIsEmpty(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            string errorMessage = $@"The value of {nameof(message)} cannot be null or empty.
                         Please provide a valid {nameof(message)}!";
            throw new InvalidOperationException(errorMessage);
        }
    }

    private void ThrowInvalidOperationExceptionIfUserIsNotAuthorized
        (string commentUsername, string requestUsername, string item)
    {
        if (!commentUsername.Equals(requestUsername, StringComparison.InvariantCultureIgnoreCase))
        {
            string errorMessage = $"You are not allowed to edit a {item} that was " +
                                  "made by another user!";
            throw new InvalidOperationException(errorMessage);
        }
    }

    public void EditMessage(string message)
    {
        ThrowInvalidOperationExceptionIfNotActive();
        ThrowInvalidOperationExceptionIfStringIsEmpty(message);

        RaiseEvent(new MessageUpdatedEvent
        {
            Id = Id,
            Message = message
        });
    }

    public void Apply(MessageUpdatedEvent @event) => Id = @event.Id;

    public void LikePost()
    {
        ThrowInvalidOperationExceptionIfNotActive();
        RaiseEvent(new PostLikedEvent
        {
            Id = Id
        });
    }
    
    public void Apply(PostLikedEvent @event) => Id = @event.Id;


    public void AddComment(string comment, string username)
    {
        ThrowInvalidOperationExceptionIfNotActive();
        ThrowInvalidOperationExceptionIfStringIsEmpty(comment);
        
        RaiseEvent(new CommentAddedEvent
        {
            Id = Id,
            CommentId = Guid.NewGuid(),
            Comment = comment,
            Username = username,
            CommentDate = DateTime.Now
        });
    }

    public void Apply(CommentAddedEvent @event)
    {
        Id = @event.Id;
        _comments.Add(@event.Id,
            new Tuple<string, string>(@event.Comment!, @event.Username!));
    }

    public void EditComment(Guid commentId, string comment, string username)
    {
        ThrowInvalidOperationExceptionIfNotActive();
        ThrowInvalidOperationExceptionIfUserIsNotAuthorized(
            _comments[commentId].Item2, 
            username, "comment");
        
        RaiseEvent(new CommentUpdatedEvent
        {
            Id = Id,
            CommentId = commentId,
            Comment = comment,
            Username = username,
            EditDate = DateTime.Now
        });
    }

    public void Apply(CommentRemovedEvent @event)
    {
        Id = @event.Id;
        _comments.Remove(@event.CommentId);
    }

    public void DeletePost(string username)
    {
        ThrowInvalidOperationExceptionIfNotActive();
        ThrowInvalidOperationExceptionIfUserIsNotAuthorized(_author!,
            username, "post");
        
        RaiseEvent(new PostRemovedEvent
        {
            Id = Id
        });
    }

    public void Apply(PostRemovedEvent @event)
    {
        Id = @event.Id;
        _active = false;
    }
}