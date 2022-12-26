using Post.Query.Domain.Entities;
using Post.Query.Domain.Repositories;

namespace Post.Query.Api.Queries;

public class QueryHandler : IQueryHandler
{
    private readonly IPostRepository _postRepository;

    public QueryHandler(IPostRepository postRepository) 
        => _postRepository = postRepository;

    public async Task<List<PostEntity>> HandleAsync(FindAllPostQuery query) 
        => await _postRepository.ListAllAsync();

    public async Task<List<PostEntity>> HandleAsync(FindPostByIdQuery query)
    {
        PostEntity? post = await _postRepository.GetByIdAsync(query.Id!.Value);
        return new List<PostEntity> {post};
    }

    public async Task<List<PostEntity>> HandleAsync(FindPostByAuthorQuery query) 
        => await _postRepository.ListByAuthorAsync(query.Author);

    public async Task<List<PostEntity>> HandleAsync(FindPostWithCommentQuery query) 
        => await _postRepository.ListWithCommentsAsync();

    public async Task<List<PostEntity>> HandleAsync(FindPostWithLikesQuery query) 
        => await _postRepository.ListWithLikesAsync(query.NumberOfLikes!.Value);
}