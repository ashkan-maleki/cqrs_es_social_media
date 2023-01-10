using Post.Common.Events;
using Post.Query.Domain.Entities;
using Post.Query.Domain.Repositories;

namespace Post.Query.Infrastructure.Handlers;

public class EventHandler : IEventHandler
{
    private readonly IPostRepository _postRepository;
    private readonly ICommentRepository _commentRepository;

    public EventHandler(IPostRepository postRepository, ICommentRepository commentRepository)
    {
        _postRepository = postRepository;
        _commentRepository = commentRepository;
    }

    public async Task On(PostCreatedEvent @event)
    {
        PostEntity post = new()
        {
            PostId = @event.Id,
            Author = @event.Author,
            DatePosted = @event.DatePosted,
            Message = @event.Message
        };
        await _postRepository.CreateAsync(post);
    }

    public async Task On(MessageUpdatedEvent @event)
    {
        PostEntity? post = await _postRepository.GetByIdAsync(@event.Id);
        if (post == null) return;
        post.Message = @event.Message;
        await _postRepository.UpdateAsync(post);
    }

    public async Task On(PostLikedEvent @event)
    {
        PostEntity? post = await _postRepository.GetByIdAsync(@event.Id);
        if (post == null) return;
        post.Likes = (post.Likes ?? 0) + 1;
        await _postRepository.UpdateAsync(post);
    }

    public async Task On(CommentAddedEvent @event)
    {
        CommentEntity? comment = new()
        {
            CommentId = @event.CommentId,
            Username = @event.Username,
            CommentDate = @event.CommentDate,
            Comment = @event.Comment,
            Edited = false,
            PostId = @event.Id
        };

        await _commentRepository.CreateAsync(comment);
    }

    public async Task On(CommentUpdatedEvent @event)
    {
        CommentEntity? comment = await _commentRepository.GetByIdAsync(@event.CommentId);
        if (comment == null) return;
        comment.Comment = @event.Comment;
        comment.Edited = true;
        comment.CommentDate = @event.EditDate;
        await _commentRepository.UpdateAsync(comment);
    }

    public async Task On(CommentRemovedEvent @event)
        => await _commentRepository.DeleteAsync(@event.CommentId);

    public async Task On(PostRemovedEvent @event)
        => await _postRepository.DeleteAsync(@event.Id);
}